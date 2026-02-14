#pragma once

#include "CoreMinimal.h"
#include "AIController.h"
#include "BehaviorTree/BehaviorTree.h"
#include "BehaviorTree/BlackboardComponent.h"
#include "FBBossAI.generated.h"

class AFBCharacter;
class AFBBoss;
class UBehaviorTreeComponent;
class UBlackboardComponent;

UENUM(BlueprintType)
enum class EBossState : uint8
{
    Idle,
    Patrol,
    Chase,
    Attack,
    SpecialAttack,
    Evade,
    Dead
};

USTRUCT(BlueprintType)
struct FBossStats
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float Health = 500.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float MaxHealth = 500.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float MovementSpeed = 400.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float AttackRange = 800.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float DetectionRange = 2000.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float AttackDamage = 25.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float AttackCooldown = 2.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float SpecialAttackCooldown = 10.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float EvadeSpeed = 800.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    int32 XPValue = 100;
};

UCLASS()
class FIREBYTE_API AFBBossAI : public AAIController
{
    GENERATED_BODY()

public:
    AFBBossAI();

    // AI Components
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    TObjectPtr<UBehaviorTreeComponent> BehaviorTreeComponent;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    TObjectPtr<UBlackboardComponent> BlackboardComponent;

    // AI Configuration
    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "AI")
    TObjectPtr<UBehaviorTree> BehaviorTree;

    // Boss stats
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Boss")
    FBossStats BossStats;

    // Blackboard keys
    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "AI")
    FName TargetPlayerKey = TEXT("TargetPlayer");

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "AI")
    FName BossStateKey = TEXT("BossState");

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "AI")
    FName LastSeenPositionKey = TEXT("LastSeenPosition");

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "AI")
    FName AttackCooldownKey = TEXT("AttackCooldown");

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "AI")
    FName SpecialAttackCooldownKey = TEXT("SpecialAttackCooldown");

    UPROPERTY(EditDefaultsOnly, BlueprintReadOnly, Category = "AI")
    FName HealthPercentageKey = TEXT("HealthPercentage");

protected:
    virtual void BeginPlay() override;
    virtual void OnPossess(APawn* InPawn) override;
    virtual void Tick(float DeltaTime) override;

    // AI state management
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "AI")
    EBossState CurrentState = EBossState::Idle;

    // Timers
    UPROPERTY()
    FTimerHandle AttackTimer;

    UPROPERTY()
    FTimerHandle SpecialAttackTimer;

    // Internal variables
    bool bHasTarget = false;
    float LastAttackTime = 0.0f;
    float LastSpecialAttackTime = 0.0f;

public:
    // AI behavior functions
    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual void SetTargetPlayer(AFBCharacter* Player);

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual void ClearTargetPlayer();

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual void UpdateBossState(EBossState NewState);

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual bool CanAttack();

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual bool CanSpecialAttack();

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual void PerformAttack();

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual void PerformSpecialAttack();

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual void Evade();

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual void ChaseTarget();

    UFUNCTION(BlueprintCallable, Category = "AI")
    virtual void Patrol();

    // Boss management
    UFUNCTION(BlueprintCallable, Category = "Boss")
    virtual void TakeDamage(float Damage);

    UFUNCTION(BlueprintCallable, Category = "Boss")
    virtual void OnDeath();

    // Utility functions
    UFUNCTION(BlueprintPure, Category = "AI")
    virtual AFBCharacter* GetTargetPlayer();

    UFUNCTION(BlueprintPure, Category = "AI")
    virtual AFBBoss* GetBossCharacter();

    UFUNCTION(BlueprintPure, Category = "AI")
    virtual float GetDistanceToTarget();

    UFUNCTION(BlueprintPure, Category = "AI")
    virtual bool IsTargetInRange(float Range);

    UFUNCTION(BlueprintPure, Category = "AI")
    virtual bool IsTargetVisible();

    UFUNCTION(BlueprintPure, Category = "AI")
    virtual FVector GetLastSeenPosition();

    // Getters
    UFUNCTION(BlueprintPure, Category = "AI")
    FORCEINLINE EBossState GetCurrentState() const { return CurrentState; }

    UFUNCTION(BlueprintPure, Category = "AI")
    FORCEINLINE bool HasTarget() const { return bHasTarget; }

    UFUNCTION(BlueprintPure, Category = "AI")
    FORCEINLINE float GetHealthPercentage() const { return BossStats.Health / BossStats.MaxHealth; }

protected:
    // Internal functions
    UFUNCTION()
    virtual void OnAttackTimer();

    UFUNCTION()
    virtual void OnSpecialAttackTimer();

    virtual void UpdateBlackboard();
    virtual void FindTargetPlayer();

    // Network replication
    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

    UFUNCTION()
    virtual void OnRep_BossStats();

    UFUNCTION()
    virtual void OnRep_CurrentState();

    // Server functions
    UFUNCTION(Server, Reliable, WithValidation)
    void Server_TakeDamage(float Damage);

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_UpdateBossState(EBossState NewState);

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_PerformAttack();

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_PerformSpecialAttack();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_OnAttack();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_OnSpecialAttack();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_OnDeath();
};
