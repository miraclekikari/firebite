#include "FBBoss.h"
#include "AI/FBBossAI.h"
#include "FBCharacter.h"
#include "Components/BoxComponent.h"
#include "Components/CapsuleComponent.h"
#include "Animation/AnimMontage.h"
#include "Animation/AnimInstance.h"
#include "NiagaraFunctionLibrary.h"
#include "Kismet/GameplayStatics.h"
#include "Kismet/KismetSystemLibrary.h"
#include "GameFramework/CharacterMovementComponent.h"
#include "Net/UnrealNetwork.h"
#include "TimerManager.h"
#include "Engine/World.h"

AFBBoss::AFBBoss()
{
    PrimaryActorTick.bCanEverTick = true;
    bReplicates = true;
    SetReplicateMovement(true);

    // Configure character movement
    if (GetCharacterMovement())
    {
        GetCharacterMovement()->bOrientRotationToMovement = true;
        GetCharacterMovement()->RotationRate = FRotator(0.0f, 500.0f, 0.0f);
        GetCharacterMovement()->MaxWalkSpeed = MovementSpeed;
        GetCharacterMovement()->JumpZVelocity = 0.0f; // Bosses don't jump
        GetCharacterMovement()->AirControl = 0.0f;
    }

    // Create melee attack collider
    MeleeAttackCollider = CreateDefaultSubobject<UBoxComponent>(TEXT("MeleeAttackCollider"));
    MeleeAttackCollider->SetupAttachment(GetMesh(), TEXT("hand_r"));
    MeleeAttackCollider->SetCollisionEnabled(ECollisionEnabled::NoCollision);
    MeleeAttackCollider->SetCollisionObjectType(ECollisionChannel::ECC_WorldDynamic);
    MeleeAttackCollider->SetCollisionResponseToAllChannels(ECR_Ignore);
    MeleeAttackCollider->SetCollisionResponseToChannel(ECC_Pawn, ECR_Overlap);

    // AI Controller class
    AIControllerClass = AFBBossAI::StaticClass();

    // Auto possess AI
    AutoPossessAI = EAutoPossessAI::PlacedInWorldOrSpawned;
}

void AFBBoss::BeginPlay()
{
    Super::BeginPlay();

    // Get AI controller
    BossAI = Cast<AFBBossAI>(GetController());

    // Bind overlap event
    MeleeAttackCollider->OnComponentBeginOverlap.AddDynamic(this, &AFBBoss::OnMeleeAttackOverlap);
}

void AFBBoss::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);
}

float AFBBoss::TakeDamage(float DamageAmount, struct FDamageEvent const& DamageEvent, class AController* EventInstigator, AActor* DamageCauser)
{
    if (Health <= 0.0f)
        return 0.0f;

    float ActualDamage = Super::TakeDamage(DamageAmount, DamageEvent, EventInstigator, DamageCauser);
    
    if (ActualDamage > 0.0f)
    {
        Health = FMath::Max(0.0f, Health - ActualDamage);
        
        // Play hurt effects
        PlayHurtEffects();

        // Notify AI controller
        if (BossAI)
        {
            BossAI->TakeDamage(ActualDamage);
        }

        if (Health <= 0.0f)
        {
            OnDeath();
        }

        if (HasAuthority())
        {
            Server_TakeDamage(ActualDamage, DamageCauser);
            OnRep_Health();
        }
    }

    return ActualDamage;
}

void AFBBoss::PerformMeleeAttack()
{
    if (bIsAttacking || bIsSpecialAttacking)
        return;

    bIsAttacking = true;

    // Play attack montage
    if (MeleeAttackMontage)
    {
        PlayAnimMontage(MeleeAttackMontage);
    }

    // Enable melee attack collider
    MeleeAttackCollider->SetCollisionEnabled(ECollisionEnabled::QueryOnly);

    // Set timer to disable attack collider
    GetWorldTimerManager().SetTimer(MeleeAttackTimer, this, &AFBBoss::OnMeleeAttackTimer, 0.5f, false);

    if (HasAuthority())
    {
        Server_PerformMeleeAttack();
        OnRep_bIsAttacking();
    }
}

void AFBBoss::PerformSpecialAttack()
{
    if (bIsAttacking || bIsSpecialAttacking)
        return;

    bIsSpecialAttacking = true;

    // Play special attack montage
    if (SpecialAttackMontage)
    {
        PlayAnimMontage(SpecialAttackMontage);
    }

    // Create special attack effect
    if (SpecialAttackEffect)
    {
        FVector EffectLocation = GetActorLocation() + FVector(0, 0, 100);
        UNiagaraFunctionLibrary::SpawnSystemAtLocation(GetWorld(), SpecialAttackEffect, EffectLocation);
    }

    // Apply damage in area
    ApplySpecialAttackDamage();

    // Set timer to end special attack
    GetWorldTimerManager().SetTimer(SpecialAttackTimer, this, &AFBBoss::OnSpecialAttackTimer, 1.5f, false);

    if (HasAuthority())
    {
        Server_PerformSpecialAttack();
        OnRep_bIsSpecialAttacking();
    }
}

void AFBBoss::StartMeleeAttack()
{
    // Called from animation notify
    ApplyMeleeDamage();
}

void AFBBoss::EndMeleeAttack()
{
    // Called from animation notify
    bIsAttacking = false;
    MeleeAttackCollider->SetCollisionEnabled(ECollisionEnabled::NoCollision);
    
    if (HasAuthority())
    {
        OnRep_bIsAttacking();
    }
}

void AFBBoss::StartSpecialAttack()
{
    // Called from animation notify
    // Additional special attack logic
}

void AFBBoss::EndSpecialAttack()
{
    // Called from animation notify
    bIsSpecialAttacking = false;
    
    if (HasAuthority())
    {
        OnRep_bIsSpecialAttacking();
    }
}

void AFBBoss::PlayAttackEffects()
{
    // Play attack sound
    if (AttackSound)
    {
        UGameplayStatics::PlaySoundAtLocation(this, AttackSound, GetActorLocation());
    }

    // Spawn attack effect
    if (MeleeAttackEffect)
    {
        FVector EffectLocation = GetMesh()->GetSocketLocation(TEXT("hand_r"));
        UNiagaraFunctionLibrary::SpawnSystemAtLocation(GetWorld(), MeleeAttackEffect, EffectLocation);
    }
}

void AFBBoss::PlaySpecialAttackEffects()
{
    // Play special attack sound
    if (SpecialAttackSound)
    {
        UGameplayStatics::PlaySoundAtLocation(this, SpecialAttackSound, GetActorLocation());
    }

    // Additional special attack effects
}

void AFBBoss::PlayDeathEffects()
{
    // Play death sound
    if (DeathSound)
    {
        UGameplayStatics::PlaySoundAtLocation(this, DeathSound, GetActorLocation());
    }

    // Spawn death effect
    if (DeathEffect)
    {
        FVector EffectLocation = GetActorLocation() + FVector(0, 0, 50);
        UNiagaraFunctionLibrary::SpawnSystemAtLocation(GetWorld(), DeathEffect, EffectLocation);
    }

    // Play death montage
    if (DeathMontage)
    {
        PlayAnimMontage(DeathMontage);
    }
}

void AFBBoss::PlayHurtEffects()
{
    // Play hurt sound
    if (HurtSound)
    {
        UGameplayStatics::PlaySoundAtLocation(this, HurtSound, GetActorLocation());
    }

    // Flash material or other hurt effects
    // This would be implemented with material instances
}

void AFBBoss::ApplyMeleeDamage()
{
    // Apply damage to overlapping actors
    TArray<AActor*> OverlappingActors;
    MeleeAttackCollider->GetOverlappingActors(OverlappingActors);

    for (AActor* Actor : OverlappingActors)
    {
        if (Actor != this && Actor != GetOwner())
        {
            // Apply damage
            UGameplayStatics::ApplyDamage(
                Actor,
                AttackDamage,
                GetInstigatorController(),
                this,
                DamageType ? DamageType : UDamageType::StaticClass()
            );
        }
    }
}

void AFBBoss::ApplySpecialAttackDamage()
{
    // Apply damage in a radius around the boss
    FVector AttackLocation = GetActorLocation();
    float AttackRadius = 500.0f;

    TArray<AActor*> OverlappingActors;
    UKismetSystemLibrary::SphereOverlapActors(
        this,
        AttackLocation,
        AttackRadius,
        TArray<TEnumAsByte<EObjectTypeQuery>>(),
        AFBCharacter::StaticClass(),
        TArray<AActor*>(),
        OverlappingActors
    );

    for (AActor* Actor : OverlappingActors)
    {
        if (Actor != this && Actor != GetOwner())
        {
            // Apply special attack damage (higher than melee)
            UGameplayStatics::ApplyDamage(
                Actor,
                AttackDamage * 2.0f,
                GetInstigatorController(),
                this,
                DamageType ? DamageType : UDamageType::StaticClass()
            );
        }
    }
}

void AFBBoss::OnDeath()
{
    // Disable collision
    GetCapsuleComponent()->SetCollisionEnabled(ECollisionEnabled::NoCollision);

    // Disable movement
    if (GetCharacterMovement())
    {
        GetCharacterMovement()->DisableMovement();
    }

    // Notify AI controller
    if (BossAI)
    {
        BossAI->OnDeath();
    }

    // Play death effects
    PlayDeathEffects();

    // Set timer to destroy actor
    FTimerHandle DeathTimer;
    FTimerDelegate DeathDelegate;
    DeathDelegate.BindLambda([this]()
    {
        Destroy();
    });
    GetWorldTimerManager().SetTimer(DeathTimer, DeathDelegate, 3.0f, false);

    if (HasAuthority())
    {
        Multicast_PlayDeathEffects();
    }
}

void AFBBoss::Respawn()
{
    // Reset boss state
    Health = MaxHealth;
    bIsAttacking = false;
    bIsSpecialAttacking = false;

    // Enable collision
    GetCapsuleComponent()->SetCollisionEnabled(ECollisionEnabled::QueryAndPhysics);

    // Enable movement
    if (GetCharacterMovement())
    {
        GetCharacterMovement()->SetMovementMode(EMovementMode::MOVE_Walking);
    }

    // Reset AI
    if (BossAI)
    {
        BossAI->UpdateBossState(EBossState::Idle);
    }
}

void AFBBoss::OnMeleeAttackOverlap(UPrimitiveComponent* OverlappedComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, int32 OtherBodyIndex, bool bFromSweep, const FHitResult& SweepResult)
{
    if (bIsAttacking && OtherActor && OtherActor != this)
    {
        // Apply damage to overlapped actor
        UGameplayStatics::ApplyDamage(
            OtherActor,
            AttackDamage,
            GetInstigatorController(),
            this,
            DamageType ? DamageType : UDamageType::StaticClass()
        );
    }
}

void AFBBoss::OnMeleeAttackTimer()
{
    EndMeleeAttack();
}

void AFBBoss::OnSpecialAttackTimer()
{
    EndSpecialAttack();
}

void AFBBoss::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);

    DOREPLIFETIME(AFBBoss, Health);
    DOREPLIFETIME(AFBBoss, MaxHealth);
    DOREPLIFETIME(AFBBoss, bIsAttacking);
    DOREPLIFETIME(AFBBoss, bIsSpecialAttacking);
}

void AFBBoss::OnRep_Health()
{
    // Update health bar or other UI elements
}

void AFBBoss::OnRep_bIsAttacking()
{
    // Update visual state for attacking
    if (bIsAttacking)
    {
        PlayAttackEffects();
    }
}

void AFBBoss::OnRep_bIsSpecialAttacking()
{
    // Update visual state for special attacking
    if (bIsSpecialAttacking)
    {
        PlaySpecialAttackEffects();
    }
}

void AFBBoss::Server_PerformMeleeAttack_Implementation()
{
    PerformMeleeAttack();
}

bool AFBBoss::Server_PerformMeleeAttack_Validate()
{
    return !bIsAttacking && !bIsSpecialAttacking;
}

void AFBBoss::Server_PerformSpecialAttack_Implementation()
{
    PerformSpecialAttack();
}

bool AFBBoss::Server_PerformSpecialAttack_Validate()
{
    return !bIsAttacking && !bIsSpecialAttacking;
}

void AFBBoss::Server_TakeDamage_Implementation(float Damage, AActor* DamageCauser)
{
    // Damage is already applied on server, this is just for replication
}

bool AFBBoss::Server_TakeDamage_Validate(float Damage, AActor* DamageCauser)
{
    return Damage > 0.0f;
}

void AFBBoss::Multicast_PlayAttackEffects_Implementation()
{
    if (!HasAuthority())
    {
        PlayAttackEffects();
    }
}

void AFBBoss::Multicast_PlaySpecialAttackEffects_Implementation()
{
    if (!HasAuthority())
    {
        PlaySpecialAttackEffects();
    }
}

void AFBBoss::Multicast_PlayDeathEffects_Implementation()
{
    if (!HasAuthority())
    {
        PlayDeathEffects();
    }
}

void AFBBoss::Multicast_PlayHurtEffects_Implementation()
{
    if (!HasAuthority())
    {
        PlayHurtEffects();
    }
}
