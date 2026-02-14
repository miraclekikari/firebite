#pragma once

#include "CoreMinimal.h"
#include "GameFramework/PlayerController.h"
#include "InputActionValue.h"
#include "FBPlayerController.generated.h"

class UInputMappingContext;
class UInputAction;
class AFBCharacter;
class UFBHUDWidget;

UENUM(BlueprintType)
enum class EPlayerState : uint8
{
    Idle,
    Moving,
    Jumping,
    Shooting,
    Reloading,
    Sprinting,
    Dead
};

UCLASS()
class FIREBYTE_API AFBPlayerController : public APlayerController
{
    GENERATED_BODY()

public:
    AFBPlayerController();

    // Player stats
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Player Stats")
    float Health = 100.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Player Stats")
    float MaxHealth = 100.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Player Stats")
    float Energy = 100.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Player Stats")
    float MaxEnergy = 100.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Player Stats")
    int32 XP = 0;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Player Stats")
    int32 Level = 1;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Player Stats")
    int32 XPToNextLevel = 100;

    // Movement settings
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
    float BaseMovementSpeed = 600.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
    float SprintMultiplier = 1.8f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
    float JumpVelocity = 800.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Movement")
    float AirControl = 0.3f;

protected:
    virtual void BeginPlay() override;
    virtual void SetupInputComponent() override;
    virtual void Tick(float DeltaTime) override;

    // Input actions
    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
    TObjectPtr<UInputMappingContext> InputMappingContext;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
    TObjectPtr<UInputAction> IA_Move;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
    TObjectPtr<UInputAction> IA_Look;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
    TObjectPtr<UInputAction> IA_Jump;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
    TObjectPtr<UInputAction> IA_Shoot;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
    TObjectPtr<UInputAction> IA_Reload;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
    TObjectPtr<UInputAction> IA_Sprint;

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "Input")
    TObjectPtr<UInputAction> IA_Interact;

    // HUD Widget
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "HUD")
    TObjectPtr<UFBHUDWidget> HUDWidget;

    // Player state
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Player State")
    EPlayerState CurrentState = EPlayerState::Idle;

private:
    // Input handlers
    void Move(const FInputActionValue& Value);
    void Look(const FInputActionValue& Value);
    void Jump(const FInputActionValue& Value);
    void Shoot(const FInputActionValue& Value);
    void Reload(const FInputActionValue& Value);
    void Sprint(const FInputActionValue& Value);
    void Interact(const FInputActionValue& Value);

    // Player management
    void HandlePlayerDeath();
    void RegenerateEnergy(float DeltaTime);
    void CheckLevelUp();

    // Character reference
    UPROPERTY()
    TObjectPtr<AFBCharacter> ControlledCharacter;

public:
    // Network replication
    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

    // Functions callable from Blueprint
    UFUNCTION(BlueprintCallable, Category = "Player")
    void TakeDamage(float DamageAmount);

    UFUNCTION(BlueprintCallable, Category = "Player")
    void Heal(float HealAmount);

    UFUNCTION(BlueprintCallable, Category = "Player")
    bool UseEnergy(float EnergyAmount);

    UFUNCTION(BlueprintCallable, Category = "Player")
    void AddXP(int32 XPAmount);

    UFUNCTION(BlueprintCallable, Category = "Player")
    void Respawn();

    UFUNCTION(BlueprintCallable, Category = "Player")
    void UpdateHUD();

    // Getters
    UFUNCTION(BlueprintPure, Category = "Player")
    FORCEINLINE EPlayerState GetCurrentState() const { return CurrentState; }

    UFUNCTION(BlueprintPure, Category = "Player")
    FORCEINLINE float GetHealthPercentage() const { return Health / MaxHealth; }

    UFUNCTION(BlueprintPure, Category = "Player")
    FORCEINLINE float GetEnergyPercentage() const { return Energy / MaxEnergy; }

    UFUNCTION(BlueprintPure, Category = "Player")
    FORCEINLINE float GetXPPercentage() const { return (float)XP / XPToNextLevel; }
};
