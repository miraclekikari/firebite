#pragma once

#include "CoreMinimal.h"
#include "GameFramework/GameModeBase.h"
#include "FBGameMode.generated.h"

class AFBPlayerController;
class AFBBoss;
class AFBCharacter;

UENUM(BlueprintType)
enum class EGameState : uint8
{
    WaitingToStart,
    InProgress,
    BossFight,
    GameOver,
    Victory
};

USTRUCT(BlueprintType)
struct FGameSettings
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
    int32 MaxPlayers = 8;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
    float GameDuration = 600.0f; // 10 minutes

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
    int32 BossSpawnInterval = 120; // Every 2 minutes

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
    int32 MaxBosses = 3;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
    float RespawnTime = 5.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
    bool bEnablePvP = false;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Settings")
    int32 ScoreToWin = 1000;
};

UCLASS()
class FIREBYTE_API AFBGameMode : public AGameModeBase
{
    GENERATED_BODY()

public:
    AFBGameMode();

    // Game configuration
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Game")
    FGameSettings GameSettings;

    // Game state
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Game")
    EGameState CurrentGameState = EGameState::WaitingToStart;

    // Player management
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Players")
    TArray<TObjectPtr<AFBPlayerController>> ConnectedPlayers;

    // Boss management
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Bosses")
    TArray<TObjectPtr<AFBBoss>> ActiveBosses;

    // Score tracking
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Score")
    int32 TotalScore = 0;

    // Timers
    UPROPERTY()
    FTimerHandle GameTimer;

    UPROPERTY()
    FTimerHandle BossSpawnTimer;

    UPROPERTY()
    FTimerHandle GameStateTimer;

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;
    virtual void PostLogin(APlayerController* NewPlayer) override;
    virtual void Logout(AController* Exiting) override;
    virtual void OnPostLogin(AController* NewPlayer) override;

    // Game flow
    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void StartGame();

    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void EndGame(bool bVictory);

    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void PauseGame();

    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void ResumeGame();

    // Player management
    UFUNCTION(BlueprintCallable, Category = "Players")
    virtual void HandlePlayerDeath(AFBPlayerController* DeadPlayer);

    UFUNCTION(BlueprintCallable, Category = "Players")
    virtual void RespawnPlayer(AFBPlayerController* Player);

    UFUNCTION(BlueprintCallable, Category = "Players")
    virtual void UpdatePlayerScore(AFBPlayerController* Player, int32 Score);

    // Boss management
    UFUNCTION(BlueprintCallable, Category = "Bosses")
    virtual void SpawnBoss();

    UFUNCTION(BlueprintCallable, Category = "Bosses")
    virtual void HandleBossDeath(AFBBoss* DeadBoss);

    UFUNCTION(BlueprintCallable, Category = "Bosses")
    virtual void StartBossFight();

    UFUNCTION(BlueprintCallable, Category = "Bosses")
    virtual void EndBossFight();

    // Game state management
    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void UpdateGameState(EGameState NewState);

    // Utility functions
    UFUNCTION(BlueprintPure, Category = "Game")
    virtual bool IsGameInProgress() const;

    UFUNCTION(BlueprintPure, Category = "Game")
    virtual int32 GetPlayerCount() const;

    UFUNCTION(BlueprintPure, Category = "Game")
    virtual int32 GetAlivePlayerCount() const;

    UFUNCTION(BlueprintPure, Category = "Game")
    virtual float GetRemainingGameTime() const;

    UFUNCTION(BlueprintPure, Category = "Game")
    virtual bool ShouldSpawnBoss() const;

    // Event dispatchers
    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnGameStateChanged OnGameStateChanged;

    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnPlayerDeath OnPlayerDeath;

    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnBossSpawned OnBossSpawned;

    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnBossDeath OnBossDeath;

    UPROPERTY(BlueprintAssignable, Category = "Events")
    FOnGameEnd OnGameEnd;

protected:
    // Internal variables
    float GameStartTime = 0.0f;
    float LastBossSpawnTime = 0.0f;
    int32 PlayersAlive = 0;

    // Internal functions
    UFUNCTION()
    virtual void OnGameTimer();

    UFUNCTION()
    virtual void OnBossSpawnTimer();

    UFUNCTION()
    virtual void OnGameStateTimer();

    virtual void InitializeGame();
    virtual void CleanupGame();

    // Network replication
    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

    UFUNCTION()
    virtual void OnRep_CurrentGameState();

    UFUNCTION()
    virtual void OnRep_TotalScore();

    // Server functions
    UFUNCTION(Server, Reliable, WithValidation)
    void Server_StartGame();

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_RespawnPlayer(AFBPlayerController* Player);

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_UpdatePlayerScore(AFBPlayerController* Player, int32 Score);

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_SpawnBoss();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_GameStateChanged(EGameState NewState);

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_PlayerDeath(AFBPlayerController* DeadPlayer);

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_BossSpawned(AFBBoss* NewBoss);

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_BossDeath(AFBBoss* DeadBoss);

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_GameEnd(bool bVictory);

public:
    // Blueprint callable functions
    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void ForceStartGame();

    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void ForceEndGame();

    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void SpawnBossAtLocation(FVector Location);

    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void GiveAllPlayersXP(int32 XPAmount);

    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void HealAllPlayers(float HealAmount);

    UFUNCTION(BlueprintCallable, Category = "Game")
    virtual void ResetGame();
};
