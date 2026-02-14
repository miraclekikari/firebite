#include "FBBossAI.h"
#include "FBCharacter.h"
#include "Boss/FBBoss.h"
#include "BehaviorTree/BlackboardComponent.h"
#include "BehaviorTree/BehaviorTreeComponent.h"
#include "Perception/AIPerceptionComponent.h"
#include "Perception/AISenseConfig_Sight.h"
#include "Kismet/KismetSystemLibrary.h"
#include "Kismet/GameplayStatics.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Net/UnrealNetwork.h"
#include "TimerManager.h"
#include "Engine/World.h"

AFBBossAI::AFBBossAI()
{
    PrimaryActorTick.bCanEverTick = true;
    bReplicates = true;

    // Create AI components
    BehaviorTreeComponent = CreateDefaultSubobject<UBehaviorTreeComponent>(TEXT("BehaviorTreeComponent"));
    BlackboardComponent = CreateDefaultSubobject<UBlackboardComponent>(TEXT("BlackboardComponent"));

    // Set perception
    PerceptionComponent = CreateDefaultSubobject<UAIPerceptionComponent>(TEXT("PerceptionComponent"));
    
    // Configure sight sense
    UAISenseConfig_Sight* SightConfig = CreateDefaultSubobject<UAISenseConfig_Sight>(TEXT("SightConfig"));
    SightConfig->SightRadius = BossStats.DetectionRange;
    SightConfig->LoseSightRadius = BossStats.DetectionRange + 500.0f;
    SightConfig->PeripheralVisionAngleDegrees = 90.0f;
    SightConfig->DetectionByAffiliation.bDetectEnemies = true;
    SightConfig->DetectionByAffiliation.bDetectNeutrals = false;
    SightConfig->DetectionByAffiliation.bDetectFriendlies = false;
    
    PerceptionComponent->ConfigureSense(*SightConfig);
    PerceptionComponent->SetDominantSense(SightConfig->GetSenseImplementation());
    PerceptionComponent->OnPerceptionUpdated.AddDynamic(this, &AFBBossAI::OnPerceptionUpdated);
}

void AFBBossAI::BeginPlay()
{
    Super::BeginPlay();

    // Initialize blackboard
    if (BehaviorTree && BlackboardComponent)
    {
        BlackboardComponent->InitializeBlackboard(*BehaviorTree->BlackboardAsset);
        BehaviorTreeComponent->StartTree(*BehaviorTree);
    }

    // Update blackboard with initial values
    UpdateBlackboard();
}

void AFBBossAI::OnPossess(APawn* InPawn)
{
    Super::OnPossess(InPawn);

    // Initialize blackboard if not already done
    if (BehaviorTree && BlackboardComponent && !BlackboardComponent->GetBlackboardAsset())
    {
        BlackboardComponent->InitializeBlackboard(*BehaviorTree->BlackboardAsset);
        BehaviorTreeComponent->StartTree(*BehaviorTree);
    }

    UpdateBlackboard();
}

void AFBBossAI::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);

    // Update AI behavior based on current state
    if (bHasTarget)
    {
        AFBCharacter* Target = GetTargetPlayer();
        if (Target)
        {
            float Distance = GetDistanceToTarget();
            
            switch (CurrentState)
            {
            case EBossState::Chase:
                if (Distance <= BossStats.AttackRange)
                {
                    UpdateBossState(EBossState::Attack);
                }
                else
                {
                    ChaseTarget();
                }
                break;
                
            case EBossState::Attack:
                if (Distance > BossStats.AttackRange)
                {
                    UpdateBossState(EBossState::Chase);
                }
                else if (CanAttack())
                {
                    PerformAttack();
                }
                break;
                
            case EBossState::Evade:
                Evade();
                break;
            }
        }
        else
        {
            ClearTargetPlayer();
            UpdateBossState(EBossState::Patrol);
        }
    }
    else
    {
        // Try to find target
        FindTargetPlayer();
        
        if (CurrentState == EBossState::Idle)
        {
            UpdateBossState(EBossState::Patrol);
        }
    }

    // Update blackboard
    UpdateBlackboard();
}

void AFBBossAI::SetTargetPlayer(AFBCharacter* Player)
{
    if (BlackboardComponent)
    {
        BlackboardComponent->SetValueAsObject(TargetPlayerKey, Player);
        bHasTarget = true;
    }
}

void AFBBossAI::ClearTargetPlayer()
{
    if (BlackboardComponent)
    {
        BlackboardComponent->ClearValue(TargetPlayerKey);
        bHasTarget = false;
    }
}

void AFBBossAI::UpdateBossState(EBossState NewState)
{
    if (CurrentState != NewState)
    {
        CurrentState = NewState;
        
        if (BlackboardComponent)
        {
            BlackboardComponent->SetValueAsEnum(BossStateKey, (uint8)NewState);
        }

        if (HasAuthority())
        {
            Server_UpdateBossState(NewState);
        }
    }
}

bool AFBBossAI::CanAttack()
{
    float CurrentTime = GetWorld()->GetTimeSeconds();
    return (CurrentTime - LastAttackTime) >= BossStats.AttackCooldown;
}

bool AFBBossAI::CanSpecialAttack()
{
    float CurrentTime = GetWorld()->GetTimeSeconds();
    return (CurrentTime - LastSpecialAttackTime) >= BossStats.SpecialAttackCooldown;
}

void AFBBossAI::PerformAttack()
{
    if (!CanAttack())
        return;

    LastAttackTime = GetWorld()->GetTimeSeconds();
    
    AFBBoss* Boss = GetBossCharacter();
    if (Boss)
    {
        // Perform melee attack
        Boss->PerformMeleeAttack();
    }

    if (HasAuthority())
    {
        Server_PerformAttack();
        Multicast_OnAttack();
    }
}

void AFBBossAI::PerformSpecialAttack()
{
    if (!CanSpecialAttack())
        return;

    LastSpecialAttackTime = GetWorld()->GetTimeSeconds();
    
    AFBBoss* Boss = GetBossCharacter();
    if (Boss)
    {
        // Perform special attack
        Boss->PerformSpecialAttack();
    }

    if (HasAuthority())
    {
        Multicast_OnSpecialAttack();
    }
}

void AFBBossAI::Evade()
{
    AFBBoss* Boss = GetBossCharacter();
    if (Boss && Boss->GetCharacterMovement())
    {
        // Calculate evade direction (away from player)
        AFBCharacter* Target = GetTargetPlayer();
        if (Target)
        {
            FVector EvadeDirection = (Boss->GetActorLocation() - Target->GetActorLocation()).GetSafeNormal();
            FVector EvadeLocation = Boss->GetActorLocation() + EvadeDirection * 500.0f;
            
            // Move to evade location
            Boss->GetCharacterMovement()->MaxWalkSpeed = BossStats.EvadeSpeed;
            MoveToLocation(EvadeLocation, 100.0f);
        }
    }
}

void AFBBossAI::ChaseTarget()
{
    AFBBoss* Boss = GetBossCharacter();
    if (Boss)
    {
        AFBCharacter* Target = GetTargetPlayer();
        if (Target)
        {
            Boss->GetCharacterMovement()->MaxWalkSpeed = BossStats.MovementSpeed;
            MoveToActor(Target, 100.0f);
        }
    }
}

void AFBBossAI::Patrol()
{
    // Simple patrol behavior - move to random points
    AFBBoss* Boss = GetBossCharacter();
    if (Boss)
    {
        Boss->GetCharacterMovement()->MaxWalkSpeed = BossStats.MovementSpeed * 0.5f;
        
        // Generate random patrol point
        FVector PatrolPoint = Boss->GetActorLocation() + FVector(
            FMath::RandRange(-1000, 1000),
            FMath::RandRange(-1000, 1000),
            0
        );
        
        MoveToLocation(PatrolPoint, 100.0f);
    }
}

void AFBBossAI::TakeDamage(float Damage)
{
    if (HasAuthority())
    {
        BossStats.Health = FMath::Max(0.0f, BossStats.Health - Damage);
        
        if (BossStats.Health <= 0.0f)
        {
            OnDeath();
        }
        else
        {
            // Chance to evade when taking damage
            if (FMath::RandRange(0.0f, 1.0f) < 0.3f)
            {
                UpdateBossState(EBossState::Evade);
            }
        }

        Server_TakeDamage(Damage);
        OnRep_BossStats();
    }
}

void AFBBossAI::OnDeath()
{
    UpdateBossState(EBossState::Dead);
    
    // Stop behavior tree
    if (BehaviorTreeComponent)
    {
        BehaviorTreeComponent->StopTree();
    }

    // Clear target
    ClearTargetPlayer();

    // Give XP to player
    AFBCharacter* Target = GetTargetPlayer();
    if (Target)
    {
        // Award XP to player
        Target->AddXP(BossStats.XPValue);
    }

    if (HasAuthority())
    {
        Multicast_OnDeath();
    }
}

AFBCharacter* AFBBossAI::GetTargetPlayer()
{
    if (BlackboardComponent)
    {
        return Cast<AFBCharacter>(BlackboardComponent->GetValueAsObject(TargetPlayerKey));
    }
    return nullptr;
}

AFBBoss* AFBBossAI::GetBossCharacter()
{
    return Cast<AFBBoss>(GetPawn());
}

float AFBBossAI::GetDistanceToTarget()
{
    AFBCharacter* Target = GetTargetPlayer();
    AFBBoss* Boss = GetBossCharacter();
    
    if (Target && Boss)
    {
        return FVector::Dist(Boss->GetActorLocation(), Target->GetActorLocation());
    }
    
    return FLT_MAX;
}

bool AFBBossAI::IsTargetInRange(float Range)
{
    return GetDistanceToTarget() <= Range;
}

bool AFBBossAI::IsTargetVisible()
{
    AFBCharacter* Target = GetTargetPlayer();
    AFBBoss* Boss = GetBossCharacter();
    
    if (Target && Boss)
    {
        FHitResult HitResult;
        FVector Start = Boss->GetActorLocation() + FVector(0, 0, 100);
        FVector End = Target->GetActorLocation() + FVector(0, 0, 100);
        
        FCollisionQueryParams QueryParams;
        QueryParams.AddIgnoredActor(Boss);
        
        return !GetWorld()->LineTraceSingleByChannel(HitResult, Start, End, ECC_Visibility, QueryParams);
    }
    
    return false;
}

FVector AFBBossAI::GetLastSeenPosition()
{
    if (BlackboardComponent)
    {
        return BlackboardComponent->GetValueAsVector(LastSeenPositionKey);
    }
    return FVector::ZeroVector;
}

void AFBBossAI::UpdateBlackboard()
{
    if (!BlackboardComponent)
        return;

    // Update boss state
    BlackboardComponent->SetValueAsEnum(BossStateKey, (uint8)CurrentState);
    
    // Update health percentage
    BlackboardComponent->SetValueAsFloat(HealthPercentageKey, GetHealthPercentage());
    
    // Update attack cooldowns
    float CurrentTime = GetWorld()->GetTimeSeconds();
    float AttackCooldown = FMath::Max(0.0f, BossStats.AttackCooldown - (CurrentTime - LastAttackTime));
    float SpecialAttackCooldown = FMath::Max(0.0f, BossStats.SpecialAttackCooldown - (CurrentTime - LastSpecialAttackTime));
    
    BlackboardComponent->SetValueAsFloat(AttackCooldownKey, AttackCooldown);
    BlackboardComponent->SetValueAsFloat(SpecialAttackCooldownKey, SpecialAttackCooldown);
}

void AFBBossAI::FindTargetPlayer()
{
    // Find nearest player
    TArray<AActor*> Players;
    UGameplayStatics::GetAllActorsOfClass(GetWorld(), AFBCharacter::StaticClass(), Players);
    
    AFBBoss* Boss = GetBossCharacter();
    if (!Boss)
        return;

    AFBCharacter* NearestPlayer = nullptr;
    float NearestDistance = FLT_MAX;

    for (AActor* Actor : Players)
    {
        AFBCharacter* Player = Cast<AFBCharacter>(Actor);
        if (Player && Player != Boss)
        {
            float Distance = FVector::Dist(Boss->GetActorLocation(), Player->GetActorLocation());
            if (Distance < NearestDistance && Distance <= BossStats.DetectionRange)
            {
                NearestDistance = Distance;
                NearestPlayer = Player;
            }
        }
    }

    if (NearestPlayer)
    {
        SetTargetPlayer(NearestPlayer);
        UpdateBossState(EBossState::Chase);
    }
}

void AFBBossAI::OnPerceptionUpdated(TArray<AActor*> UpdatedActors)
{
    for (AActor* Actor : UpdatedActors)
    {
        AFBCharacter* Player = Cast<AFBCharacter>(Actor);
        if (Player)
        {
            if (IsTargetVisible())
            {
                SetTargetPlayer(Player);
                UpdateBossState(EBossState::Chase);
            }
            else
            {
                // Store last seen position
                if (BlackboardComponent)
                {
                    BlackboardComponent->SetValueAsVector(LastSeenPositionKey, Player->GetActorLocation());
                }
            }
        }
    }
}

void AFBBossAI::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);

    DOREPLIFETIME(AFBBossAI, BossStats);
    DOREPLIFETIME(AFBBossAI, CurrentState);
    DOREPLIFETIME(AFBBossAI, bHasTarget);
}

void AFBBossAI::OnRep_BossStats()
{
    UpdateBlackboard();
}

void AFBBossAI::OnRep_CurrentState()
{
    UpdateBlackboard();
}

void AFBBossAI::Server_TakeDamage_Implementation(float Damage)
{
    TakeDamage(Damage);
}

bool AFBBossAI::Server_TakeDamage_Validate(float Damage)
{
    return Damage > 0.0f;
}

void AFBBossAI::Server_UpdateBossState_Implementation(EBossState NewState)
{
    UpdateBossState(NewState);
}

bool AFBBossAI::Server_UpdateBossState_Validate(EBossState NewState)
{
    return true;
}

void AFBBossAI::Server_PerformAttack_Implementation()
{
    PerformAttack();
}

bool AFBBossAI::Server_PerformAttack_Validate()
{
    return true;
}

void AFBBossAI::Multicast_OnAttack_Implementation()
{
    // Play attack effects on all clients
    AFBBoss* Boss = GetBossCharacter();
    if (Boss)
    {
        Boss->PlayAttackEffects();
    }
}

void AFBBossAI::Multicast_OnSpecialAttack_Implementation()
{
    // Play special attack effects on all clients
    AFBBoss* Boss = GetBossCharacter();
    if (Boss)
    {
        Boss->PlaySpecialAttackEffects();
    }
}

void AFBBossAI::Multicast_OnDeath_Implementation()
{
    // Play death effects on all clients
    AFBBoss* Boss = GetBossCharacter();
    if (Boss)
    {
        Boss->PlayDeathEffects();
    }
}
