#include "FBGameMode.h"
#include "Player/FBPlayerController.h"
#include "Boss/FBBoss.h"
#include "FBCharacter.h"
#include "Kismet/GameplayStatics.h"
#include "Engine/World.h"
#include "TimerManager.h"
#include "Net/UnrealNetwork.h"

AFBGameMode::AFBGameMode()
{
    PrimaryActorTick.bCanEverTick = true;
    bReplicates = true;

    // Set default player controller class
    PlayerControllerClass = AFBPlayerController::StaticClass();

    // Set default pawn class
    DefaultPawnClass = AFBCharacter::StaticClass();

    // Initialize game settings
    GameSettings.MaxPlayers = 8;
    GameSettings.GameDuration = 600.0f;
    GameSettings.BossSpawnInterval = 120;
    GameSettings.MaxBosses = 3;
    GameSettings.RespawnTime = 5.0f;
    GameSettings.bEnablePvP = false;
    GameSettings.ScoreToWin = 1000;
}

void AFBGameMode::BeginPlay()
{
    Super::BeginPlay();

    InitializeGame();
}

void AFBGameMode::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    if (CurrentGameState == EGameState::InProgress)
    {
        // Check win conditions
        if (TotalScore >= GameSettings.ScoreToWin)
        {
            EndGame(true);
        }
        else if (GetAlivePlayerCount() == 0)
        {
            EndGame(false);
        }
        else if (ShouldSpawnBoss())
        {
            SpawnBoss();
        }
    }
}

void AFBGameMode::PostLogin(APlayerController* NewPlayer)
{
    Super::PostLogin(NewPlayer);

    AFBPlayerController* FBPlayer = Cast<AFBPlayerController>(NewPlayer);
    if (FBPlayer)
    {
        ConnectedPlayers.Add(FBPlayer);
        PlayersAlive++;

        UE_LOG(LogTemp, Log, TEXT("Player joined. Total players: %d"), ConnectedPlayers.Num());

        // Start game if we have enough players
        if (ConnectedPlayers.Num() >= 1 && CurrentGameState == EGameState::WaitingToStart)
        {
            StartGame();
        }
    }
}

void AFBGameMode::Logout(AController* Exiting)
{
    Super::Logout(Exiting);

    AFBPlayerController* FBPlayer = Cast<AFBPlayerController>(Exiting);
    if (FBPlayer)
    {
        ConnectedPlayers.Remove(FBPlayer);
        UE_LOG(LogTemp, Log, TEXT("Player left. Total players: %d"), ConnectedPlayers.Num());
    }
}

void AFBGameMode::OnPostLogin(AController* NewPlayer)
{
    Super::OnPostLogin(NewPlayer);

    // Additional post-login logic
    AFBPlayerController* FBPlayer = Cast<AFBPlayerController>(NewPlayer);
    if (FBPlayer)
    {
        // Initialize player for the game
        if (CurrentGameState == EGameState::InProgress)
        {
            // Player joined mid-game, spawn them
            RespawnPlayer(FBPlayer);
        }
    }
}

void AFBGameMode::StartGame()
{
    if (!HasAuthority())
        return;

    UpdateGameState(EGameState::InProgress);
    GameStartTime = GetWorld()->GetTimeSeconds();

    // Start game timer
    GetWorldTimerManager().SetTimer(
        GameTimer,
        this,
        &AFBGameMode::OnGameTimer,
        GameSettings.GameDuration,
        false
    );

    // Start boss spawn timer
    GetWorldTimerManager().SetTimer(
        BossSpawnTimer,
        this,
        &AFBGameMode::OnBossSpawnTimer,
        GameSettings.BossSpawnInterval,
        true
    );

    // Spawn initial boss
    SpawnBoss();

    UE_LOG(LogTemp, Log, TEXT("Game started!"));
}

void AFBGameMode::EndGame(bool bVictory)
{
    if (!HasAuthority())
        return;

    UpdateGameState(bVictory ? EGameState::Victory : EGameState::GameOver);

    // Clear all timers
    GetWorldTimerManager().ClearTimer(GameTimer);
    GetWorldTimerManager().ClearTimer(BossSpawnTimer);

    // Notify all players
    Multicast_GameEnd(bVictory);

    UE_LOG(LogTemp, Log, TEXT("Game ended. Victory: %s"), bVictory ? TEXT("True") : TEXT("False"));
}

void AFBGameMode::PauseGame()
{
    // Implementation for pausing the game
    UGameplayStatics::SetGamePaused(this, true);
}

void AFBGameMode::ResumeGame()
{
    // Implementation for resuming the game
    UGameplayStatics::SetGamePaused(this, false);
}

void AFBGameMode::HandlePlayerDeath(AFBPlayerController* DeadPlayer)
{
    if (!DeadPlayer)
        return;

    PlayersAlive--;
    
    UE_LOG(LogTemp, Log, TEXT("Player died. Players alive: %d"), PlayersAlive);

    // Notify other players
    Multicast_PlayerDeath(DeadPlayer);

    // Check game over condition
    if (PlayersAlive <= 0)
    {
        EndGame(false);
    }

    // Schedule respawn
    GetWorldTimerManager().SetTimer(
        GameStateTimer,
        [this, DeadPlayer]()
        {
            RespawnPlayer(DeadPlayer);
        },
        GameSettings.RespawnTime,
        false
    );
}

void AFBGameMode::RespawnPlayer(AFBPlayerController* Player)
{
    if (!Player || !HasAuthority())
        return;

    // Find a spawn point
    FVector SpawnLocation = FVector(0, 0, 200);
    FRotator SpawnRotation = FRotator::ZeroRotator;

    AActor* StartSpot = FindPlayerStart(Player);
    if (StartSpot)
    {
        SpawnLocation = StartSpot->GetActorLocation();
        SpawnRotation = StartSpot->GetActorRotation();
    }

    // Respawn player character
    if (AFBCharacter* Character = Cast<AFBCharacter>(Player->GetCharacter()))
    {
        Character->SetActorLocationAndRotation(SpawnLocation, SpawnRotation);
        Character->Respawn();
    }
    else
    {
        // Spawn new character if none exists
        FActorSpawnParameters SpawnParams;
        SpawnParams.Owner = Player;
        SpawnParams.Instigator = Player->GetInstigator();
        
        AFBCharacter* NewCharacter = GetWorld()->SpawnActor<AFBCharacter>(DefaultPawnClass, SpawnLocation, SpawnRotation, SpawnParams);
        if (NewCharacter)
        {
            Player->Possess(NewCharacter);
        }
    }

    PlayersAlive++;
    UE_LOG(LogTemp, Log, TEXT("Player respawned. Players alive: %d"), PlayersAlive);
}

void AFBGameMode::UpdatePlayerScore(AFBPlayerController* Player, int32 Score)
{
    if (!Player || !HasAuthority())
        return;

    TotalScore += Score;
    
    UE_LOG(LogTemp, Log, TEXT("Player score updated. Total score: %d"), TotalScore);

    // Check win condition
    if (TotalScore >= GameSettings.ScoreToWin)
    {
        EndGame(true);
    }
}

void AFBGameMode::SpawnBoss()
{
    if (!HasAuthority() || ActiveBosses.Num() >= GameSettings.MaxBosses)
        return;

    // Find spawn location
    FVector SpawnLocation = FVector(FMath::RandRange(-2000, 2000), FMath::RandRange(-2000, 2000), 200);
    FRotator SpawnRotation = FRotator::ZeroRotator;

    // Spawn boss
    FActorSpawnParameters SpawnParams;
    SpawnParams.Instigator = GetInstigator();
    SpawnParams.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AdjustIfPossibleButAlwaysSpawn;

    AFBBoss* NewBoss = GetWorld()->SpawnActor<AFBBoss>(AFBBoss::StaticClass(), SpawnLocation, SpawnRotation, SpawnParams);
    if (NewBoss)
    {
        ActiveBosses.Add(NewBoss);
        LastBossSpawnTime = GetWorld()->GetTimeSeconds();

        UE_LOG(LogTemp, Log, TEXT("Boss spawned. Active bosses: %d"), ActiveBosses.Num());

        // Notify players
        Multicast_BossSpawned(NewBoss);

        // Start boss fight if this is the first boss
        if (ActiveBosses.Num() == 1)
        {
            StartBossFight();
        }
    }
}

void AFBGameMode::HandleBossDeath(AFBBoss* DeadBoss)
{
    if (!DeadBoss)
        return;

    ActiveBosses.Remove(DeadBoss);
    
    UE_LOG(LogTemp, Log, TEXT("Boss defeated. Active bosses: %d"), ActiveBosses.Num());

    // Notify players
    Multicast_BossDeath(DeadBoss);

    // Give all players XP
    GiveAllPlayersXP(DeadBoss->XPValue);

    // Update score
    TotalScore += DeadBoss->XPValue;

    // End boss fight if no more bosses
    if (ActiveBosses.Num() == 0)
    {
        EndBossFight();
    }
}

void AFBGameMode::StartBossFight()
{
    UpdateGameState(EGameState::BossFight);
    UE_LOG(LogTemp, Log, TEXT("Boss fight started!"));
}

void AFBGameMode::EndBossFight()
{
    if (CurrentGameState == EGameState::BossFight)
    {
        UpdateGameState(EGameState::InProgress);
        UE_LOG(LogTemp, Log, TEXT("Boss fight ended!"));
    }
}

void AFBGameMode::UpdateGameState(EGameState NewState)
{
    if (CurrentGameState != NewState)
    {
        CurrentGameState = NewState;
        
        UE_LOG(LogTemp, Log, TEXT("Game state changed to: %d"), (int32)NewState);

        if (HasAuthority())
        {
            OnRep_CurrentGameState();
            Multicast_GameStateChanged(NewState);
        }
    }
}

bool AFBGameMode::IsGameInProgress() const
{
    return CurrentGameState == EGameState::InProgress || CurrentGameState == EGameState::BossFight;
}

int32 AFBGameMode::GetPlayerCount() const
{
    return ConnectedPlayers.Num();
}

int32 AFBGameMode::GetAlivePlayerCount() const
{
    return PlayersAlive;
}

float AFBGameMode::GetRemainingGameTime() const
{
    if (CurrentGameState == EGameState::InProgress || CurrentGameState == EGameState::BossFight)
    {
        float ElapsedTime = GetWorld()->GetTimeSeconds() - GameStartTime;
        return FMath::Max(0.0f, GameSettings.GameDuration - ElapsedTime);
    }
    return 0.0f;
}

bool AFBGameMode::ShouldSpawnBoss() const
{
    if (!IsGameInProgress() || ActiveBosses.Num() >= GameSettings.MaxBosses)
        return false;

    float CurrentTime = GetWorld()->GetTimeSeconds();
    return (CurrentTime - LastBossSpawnTime) >= GameSettings.BossSpawnInterval;
}

void AFBGameMode::OnGameTimer()
{
    EndGame(false); // Time's up
}

void AFBGameMode::OnBossSpawnTimer()
{
    if (ShouldSpawnBoss())
    {
        SpawnBoss();
    }
}

void AFBGameMode::OnGameStateTimer()
{
    // Generic timer for various game state operations
}

void AFBGameMode::InitializeGame()
{
    PlayersAlive = 0;
    TotalScore = 0;
    ActiveBosses.Empty();
    CurrentGameState = EGameState::WaitingToStart;

    UE_LOG(LogTemp, Log, TEXT("Game initialized"));
}

void AFBGameMode::CleanupGame()
{
    // Clean up all active actors
    for (AFBBoss* Boss : ActiveBosses)
    {
        if (Boss)
        {
            Boss->Destroy();
        }
    }
    ActiveBosses.Empty();

    UE_LOG(LogTemp, Log, TEXT("Game cleaned up"));
}

void AFBGameMode::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);

    DOREPLIFETIME(AFBGameMode, CurrentGameState);
    DOREPLIFETIME(AFBGameMode, TotalScore);
    DOREPLIFETIME(AFBGameMode, ActiveBosses);
}

void AFBGameMode::OnRep_CurrentGameState()
{
    // Handle game state changes on clients
}

void AFBGameMode::OnRep_TotalScore()
{
    // Handle score changes on clients
}

void AFBGameMode::Server_StartGame_Implementation()
{
    StartGame();
}

bool AFBGameMode::Server_StartGame_Validate()
{
    return CurrentGameState == EGameState::WaitingToStart;
}

void AFBGameMode::Server_RespawnPlayer_Implementation(AFBPlayerController* Player)
{
    RespawnPlayer(Player);
}

bool AFBGameMode::Server_RespawnPlayer_Validate(AFBPlayerController* Player)
{
    return Player != nullptr;
}

void AFBGameMode::Server_UpdatePlayerScore_Implementation(AFBPlayerController* Player, int32 Score)
{
    UpdatePlayerScore(Player, Score);
}

bool AFBGameMode::Server_UpdatePlayerScore_Validate(AFBPlayerController* Player, int32 Score)
{
    return Player != nullptr && Score > 0;
}

void AFBGameMode::Server_SpawnBoss_Implementation()
{
    SpawnBoss();
}

bool AFBGameMode::Server_SpawnBoss_Validate()
{
    return IsGameInProgress() && ActiveBosses.Num() < GameSettings.MaxBosses;
}

void AFBGameMode::Multicast_GameStateChanged_Implementation(EGameState NewState)
{
    // Notify all clients of game state change
}

void AFBGameMode::Multicast_PlayerDeath_Implementation(AFBPlayerController* DeadPlayer)
{
    // Notify all clients of player death
}

void AFBGameMode::Multicast_BossSpawned_Implementation(AFBBoss* NewBoss)
{
    // Notify all clients of boss spawn
}

void AFBGameMode::Multicast_BossDeath_Implementation(AFBBoss* DeadBoss)
{
    // Notify all clients of boss death
}

void AFBGameMode::Multicast_GameEnd_Implementation(bool bVictory)
{
    // Notify all clients of game end
}

void AFBGameMode::ForceStartGame()
{
    if (HasAuthority())
    {
        StartGame();
    }
}

void AFBGameMode::ForceEndGame()
{
    if (HasAuthority())
    {
        EndGame(false);
    }
}

void AFBGameMode::SpawnBossAtLocation(FVector Location)
{
    if (!HasAuthority())
        return;

    if (ActiveBosses.Num() >= GameSettings.MaxBosses)
        return;

    FRotator SpawnRotation = FRotator::ZeroRotator;
    FActorSpawnParameters SpawnParams;
    SpawnParams.Instigator = GetInstigator();
    SpawnParams.SpawnCollisionHandlingOverride = ESpawnActorCollisionHandlingMethod::AdjustIfPossibleButAlwaysSpawn;

    AFBBoss* NewBoss = GetWorld()->SpawnActor<AFBBoss>(AFBBoss::StaticClass(), Location, SpawnRotation, SpawnParams);
    if (NewBoss)
    {
        ActiveBosses.Add(NewBoss);
        LastBossSpawnTime = GetWorld()->GetTimeSeconds();
        Multicast_BossSpawned(NewBoss);
    }
}

void AFBGameMode::GiveAllPlayersXP(int32 XPAmount)
{
    for (AFBPlayerController* Player : ConnectedPlayers)
    {
        if (Player && Player->GetCharacter())
        {
            Player->AddXP(XPAmount);
        }
    }
}

void AFBGameMode::HealAllPlayers(float HealAmount)
{
    for (AFBPlayerController* Player : ConnectedPlayers)
    {
        if (Player)
        {
            Player->Heal(HealAmount);
        }
    }
}

void AFBGameMode::ResetGame()
{
    if (!HasAuthority())
        return;

    CleanupGame();
    InitializeGame();
    
    // Respawn all players
    for (AFBPlayerController* Player : ConnectedPlayers)
    {
        RespawnPlayer(Player);
    }
}
