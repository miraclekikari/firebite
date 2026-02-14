#include "FBPlayerController.h"
#include "FBCharacter.h"
#include "HUD/FBHUDWidget.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Engine/World.h"
#include "Kismet/GameplayStatics.h"
#include "Net/UnrealNetwork.h"

AFBPlayerController::AFBPlayerController()
{
    PrimaryActorTick.bCanEverTick = true;
    bReplicates = true;
}

void AFBPlayerController::BeginPlay()
{
    Super::BeginPlay();

    // Get controlled character
    ControlledCharacter = Cast<AFBCharacter>(GetCharacter());

    // Set up enhanced input
    if (UEnhancedInputLocalPlayerSubsystem* Subsystem = ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(GetLocalPlayer()))
    {
        if (InputMappingContext)
        {
            Subsystem->AddMappingContext(InputMappingContext, 0);
        }
    }

    // Create and initialize HUD
    if (HUDWidgetClass)
    {
        HUDWidget = CreateWidget<UFBHUDWidget>(this, HUDWidgetClass);
        if (HUDWidget)
        {
            HUDWidget->AddToViewport();
            UpdateHUD();
        }
    }
}

void AFBPlayerController::SetupInputComponent()
{
    Super::SetupInputComponent();

    if (UEnhancedInputComponent* EnhancedInputComponent = Cast<UEnhancedInputComponent>(InputComponent))
    {
        // Movement
        EnhancedInputComponent->BindAction(IA_Move, ETriggerEvent::Triggered, this, &AFBPlayerController::Move);
        EnhancedInputComponent->BindAction(IA_Look, ETriggerEvent::Triggered, this, &AFBPlayerController::Look);
        
        // Actions
        EnhancedInputComponent->BindAction(IA_Jump, ETriggerEvent::Started, this, &AFBPlayerController::Jump);
        EnhancedInputComponent->BindAction(IA_Shoot, ETriggerEvent::Started, this, &AFBPlayerController::Shoot);
        EnhancedInputComponent->BindAction(IA_Reload, ETriggerEvent::Started, this, &AFBPlayerController::Reload);
        EnhancedInputComponent->BindAction(IA_Sprint, ETriggerEvent::Started, this, &AFBPlayerController::Sprint);
        EnhancedInputComponent->BindAction(IA_Sprint, ETriggerEvent::Completed, this, &AFBPlayerController::Sprint);
        EnhancedInputComponent->BindAction(IA_Interact, ETriggerEvent::Started, this, &AFBPlayerController::Interact);
    }
}

void AFBPlayerController::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    // Regenerate energy
    RegenerateEnergy(DeltaTime);

    // Update HUD
    if (HUDWidget)
    {
        UpdateHUD();
    }
}

void AFBPlayerController::Move(const FInputActionValue& Value)
{
    if (!ControlledCharacter) return;

    const FVector2D MovementVector = Value.Get<FVector2D>();
    
    if (MovementVector.X != 0.f)
    {
        ControlledCharacter->AddMovementInput(ControlledCharacter->GetActorRightVector(), MovementVector.X);
        if (CurrentState == EPlayerState::Idle)
        {
            CurrentState = EPlayerState::Moving;
        }
    }

    if (MovementVector.Y != 0.f)
    {
        ControlledCharacter->AddMovementInput(ControlledCharacter->GetActorForwardVector(), MovementVector.Y);
        if (CurrentState == EPlayerState::Idle)
        {
            CurrentState = EPlayerState::Moving;
        }
    }

    if (MovementVector.X == 0.f && MovementVector.Y == 0.f && CurrentState == EPlayerState::Moving)
    {
        CurrentState = EPlayerState::Idle;
    }
}

void AFBPlayerController::Look(const FInputActionValue& Value)
{
    if (!ControlledCharacter) return;

    const FVector2D LookVector = Value.Get<FVector2D>();

    // Add yaw and pitch input
    AddYawInput(LookVector.X);
    AddPitchInput(LookVector.Y);
}

void AFBPlayerController::Jump(const FInputActionValue& Value)
{
    if (!ControlledCharacter) return;

    if (Value.Get<bool>())
    {
        ControlledCharacter->Jump();
        CurrentState = EPlayerState::Jumping;
    }
}

void AFBPlayerController::Shoot(const FInputActionValue& Value)
{
    if (!ControlledCharacter) return;

    if (Value.Get<bool>())
    {
        CurrentState = EPlayerState::Shooting;
        // Delegate shooting to character
        ControlledCharacter->Shoot();
    }
}

void AFBPlayerController::Reload(const FInputActionValue& Value)
{
    if (!ControlledCharacter) return;

    if (Value.Get<bool>())
    {
        CurrentState = EPlayerState::Reloading;
        // Delegate reload to character
        ControlledCharacter->Reload();
    }
}

void AFBPlayerController::Sprint(const FInputActionValue& Value)
{
    if (!ControlledCharacter) return;

    if (Value.Get<bool>())
    {
        CurrentState = EPlayerState::Sprinting;
        if (UCharacterMovementComponent* MovementComp = ControlledCharacter->GetCharacterMovement())
        {
            MovementComp->MaxWalkSpeed = BaseMovementSpeed * SprintMultiplier;
        }
    }
    else
    {
        if (CurrentState == EPlayerState::Sprinting)
        {
            CurrentState = EPlayerState::Moving;
        }
        if (UCharacterMovementComponent* MovementComp = ControlledCharacter->GetCharacterMovement())
        {
            MovementComp->MaxWalkSpeed = BaseMovementSpeed;
        }
    }
}

void AFBPlayerController::Interact(const FInputActionValue& Value)
{
    if (!ControlledCharacter) return;

    if (Value.Get<bool>())
    {
        // Perform interaction
        ControlledCharacter->Interact();
    }
}

void AFBPlayerController::TakeDamage(float DamageAmount)
{
    if (HasAuthority())
    {
        Health = FMath::Max(0.0f, Health - DamageAmount);
        
        if (Health <= 0.0f)
        {
            HandlePlayerDeath();
        }

        OnRep_Health();
    }
}

void AFBPlayerController::Heal(float HealAmount)
{
    if (HasAuthority())
    {
        Health = FMath::Min(MaxHealth, Health + HealAmount);
        OnRep_Health();
    }
}

bool AFBPlayerController::UseEnergy(float EnergyAmount)
{
    if (Energy >= EnergyAmount)
    {
        if (HasAuthority())
        {
            Energy -= EnergyAmount;
            OnRep_Energy();
        }
        return true;
    }
    return false;
}

void AFBPlayerController::AddXP(int32 XPAmount)
{
    if (HasAuthority())
    {
        XP += XPAmount;
        CheckLevelUp();
        OnRep_XP();
    }
}

void AFBPlayerController::HandlePlayerDeath()
{
    CurrentState = EPlayerState::Dead;
    
    // Disable input
    DisableInput(this);

    // Start respawn timer
    FTimerHandle RespawnTimer;
    FTimerDelegate RespawnDelegate;
    RespawnDelegate.BindUObject(this, &AFBPlayerController::Respawn);
    GetWorldTimerManager().SetTimer(RespawnTimer, RespawnDelegate, 3.0f, false);
}

void AFBPlayerController::Respawn()
{
    Health = MaxHealth;
    Energy = MaxEnergy;
    CurrentState = EPlayerState::Idle;
    
    // Enable input
    EnableInput(this);

    // Respawn character (implementation depends on game mode)
    if (AGameModeBase* GameMode = GetWorld()->GetAuthGameMode())
    {
        // Respawn logic would be handled by game mode
        UE_LOG(LogTemp, Log, TEXT("Player respawned"));
    }
}

void AFBPlayerController::RegenerateEnergy(float DeltaTime)
{
    if (HasAuthority() && Energy < MaxEnergy)
    {
        const float EnergyRegenRate = 10.0f; // 10 energy per second
        Energy = FMath::Min(MaxEnergy, Energy + EnergyRegenRate * DeltaTime);
        OnRep_Energy();
    }
}

void AFBPlayerController::CheckLevelUp()
{
    while (XP >= XPToNextLevel)
    {
        XP -= XPToNextLevel;
        Level++;
        XPToNextLevel = FMath::RoundToFloat(XPToNextLevel * 1.5f);
        
        // Increase stats on level up
        MaxHealth += 10.0f;
        Health = MaxHealth;
        MaxEnergy += 5.0f;
        Energy = MaxEnergy;
        
        UE_LOG(LogTemp, Log, TEXT("Level up! Now level %d"), Level);
    }
}

void AFBPlayerController::UpdateHUD()
{
    if (HUDWidget)
    {
        HUDWidget->UpdatePlayerStats(GetHealthPercentage(), GetEnergyPercentage(), GetXPPercentage(), Level);
    }
}

void AFBPlayerController::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);

    DOREPLIFETIME(AFBPlayerController, Health);
    DOREPLIFETIME(AFBPlayerController, MaxHealth);
    DOREPLIFETIME(AFBPlayerController, Energy);
    DOREPLIFETIME(AFBPlayerController, MaxEnergy);
    DOREPLIFETIME(AFBPlayerController, XP);
    DOREPLIFETIME(AFBPlayerController, Level);
    DOREPLIFETIME(AFBPlayerController, XPToNextLevel);
    DOREPLIFETIME(AFBPlayerController, CurrentState);
}

void AFBPlayerController::OnRep_Health()
{
    UpdateHUD();
}

void AFBPlayerController::OnRep_Energy()
{
    UpdateHUD();
}

void AFBPlayerController::OnRep_XP()
{
    UpdateHUD();
}
