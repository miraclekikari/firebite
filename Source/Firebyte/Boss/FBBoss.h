#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Character.h"
#include "Engine/EngineTypes.h"
#include "FBBoss.generated.h"

class UAnimMontage;
class UNiagaraSystem;
class USoundBase;
class UBoxComponent;
class UFBBossAI;

UENUM(BlueprintType)
enum class EBossAttackType : uint8
{
    Melee,
    Ranged,
    Special,
    Ultimate
};

UCLASS(Blueprintable, BlueprintType)
class FIREBYTE_API AFBBoss : public ACharacter
{
    GENERATED_BODY()

public:
    AFBBoss();

    // Boss components
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components")
    TObjectPtr<UBoxComponent> MeleeAttackCollider;

    // Boss configuration
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
    float Health = 500.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
    float MaxHealth = 500.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
    float MovementSpeed = 400.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
    float AttackRange = 300.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
    float AttackDamage = 25.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
    int32 XPValue = 100;

    // Attack configuration
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Combat")
    TSubclassOf<UDamageType> DamageType;

    // Visual effects
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<UNiagaraSystem> MeleeAttackEffect;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<UNiagaraSystem> SpecialAttackEffect;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<UNiagaraSystem> DeathEffect;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<USoundBase> AttackSound;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<USoundBase> SpecialAttackSound;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<USoundBase> DeathSound;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<USoundBase> HurtSound;

    // Animations
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animations")
    TObjectPtr<UAnimMontage> MeleeAttackMontage;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animations")
    TObjectPtr<UAnimMontage> SpecialAttackMontage;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animations")
    TObjectPtr<UAnimMontage> DeathMontage;

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;
    virtual float TakeDamage(float DamageAmount, struct FDamageEvent const& DamageEvent, class AController* EventInstigator, AActor* DamageCauser) override;

    // AI Controller
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    TObjectPtr<UFBBossAI> BossAI;

    // Attack state
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Combat")
    bool bIsAttacking = false;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Combat")
    bool bIsSpecialAttacking = false;

    // Timers
    UPROPERTY()
    FTimerHandle MeleeAttackTimer;

    UPROPERTY()
    FTimerHandle SpecialAttackTimer;

public:
    // Combat functions
    UFUNCTION(BlueprintCallable, Category = "Combat")
    virtual void PerformMeleeAttack();

    UFUNCTION(BlueprintCallable, Category = "Combat")
    virtual void PerformSpecialAttack();

    UFUNCTION(BlueprintCallable, Category = "Combat")
    virtual void StartMeleeAttack();

    UFUNCTION(BlueprintCallable, Category = "Combat")
    virtual void EndMeleeAttack();

    UFUNCTION(BlueprintCallable, Category = "Combat")
    virtual void StartSpecialAttack();

    UFUNCTION(BlueprintCallable, Category = "Combat")
    virtual void EndSpecialAttack();

    // Visual effects
    UFUNCTION(BlueprintCallable, Category = "Effects")
    virtual void PlayAttackEffects();

    UFUNCTION(BlueprintCallable, Category = "Effects")
    virtual void PlaySpecialAttackEffects();

    UFUNCTION(BlueprintCallable, Category = "Effects")
    virtual void PlayDeathEffects();

    UFUNCTION(BlueprintCallable, Category = "Effects")
    virtual void PlayHurtEffects();

    // Damage handling
    UFUNCTION(BlueprintCallable, Category = "Combat")
    virtual void ApplyMeleeDamage();

    // Boss management
    UFUNCTION(BlueprintCallable, Category = "Boss")
    virtual void OnDeath();

    UFUNCTION(BlueprintCallable, Category = "Boss")
    virtual void Respawn();

    // Getters
    UFUNCTION(BlueprintPure, Category = "Boss")
    FORCEINLINE float GetHealthPercentage() const { return Health / MaxHealth; }

    UFUNCTION(BlueprintPure, Category = "Boss")
    FORCEINLINE bool IsAttacking() const { return bIsAttacking; }

    UFUNCTION(BlueprintPure, Category = "Boss")
    FORCEINLINE bool IsSpecialAttacking() const { return bIsSpecialAttacking; }

protected:
    // Collision handling
    UFUNCTION()
    virtual void OnMeleeAttackOverlap(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult);

    // Internal functions
    UFUNCTION()
    virtual void OnMeleeAttackTimer();

    UFUNCTION()
    virtual void OnSpecialAttackTimer();

    // Network replication
    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

    UFUNCTION()
    virtual void OnRep_Health();

    UFUNCTION()
    virtual void OnRep_bIsAttacking();

    UFUNCTION()
    virtual void OnRep_bIsSpecialAttacking();

    // Server functions
    UFUNCTION(Server, Reliable, WithValidation)
    void Server_PerformMeleeAttack();

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_PerformSpecialAttack();

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_TakeDamage(float Damage, AActor* DamageCauser);

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_PlayAttackEffects();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_PlaySpecialAttackEffects();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_PlayDeathEffects();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_PlayHurtEffects();
};
