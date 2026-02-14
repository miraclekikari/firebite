#include "FBBullet.h"
#include "Components/SphereComponent.h"
#include "GameFramework/ProjectileMovementComponent.h"
#include "NiagaraFunctionLibrary.h"
#include "Kismet/GameplayStatics.h"
#include "Net/UnrealNetwork.h"

AFBBullet::AFBBullet()
{
    PrimaryActorTick.bCanEverTick = true;
    bReplicates = true;
    SetReplicateMovement(true);

    // Create collision component
    CollisionComponent = CreateDefaultSubobject<USphereComponent>(TEXT("CollisionComponent"));
    RootComponent = CollisionComponent;
    CollisionComponent->InitSphereRadius(2.0f);
    CollisionComponent->SetCollisionEnabled(ECollisionEnabled::QueryOnly);
    CollisionComponent->SetCollisionObjectType(ECollisionChannel::ECC_WorldDynamic);
    CollisionComponent->SetCollisionResponseToAllChannels(ECR_Ignore);
    CollisionComponent->SetCollisionResponseToChannel(ECC_WorldStatic, ECR_Block);
    CollisionComponent->SetCollisionResponseToChannel(ECC_WorldDynamic, ECR_Block);
    CollisionComponent->SetCollisionResponseToChannel(ECC_Pawn, ECR_Block);

    // Create projectile movement component
    ProjectileMovement = CreateDefaultSubobject<UProjectileMovementComponent>(TEXT("ProjectileMovement"));
    ProjectileMovement->UpdatedComponent = CollisionComponent;
    ProjectileMovement->InitialSpeed = Speed;
    ProjectileMovement->MaxSpeed = Speed;
    ProjectileMovement->bRotationFollowsVelocity = true;
    ProjectileMovement->bShouldBounce = false;
    ProjectileMovement->ProjectileGravityScale = 0.0f; // No gravity for bullets

    // Set default lifespan
    InitialLifeSpan = Lifespan;
}

void AFBBullet::BeginPlay()
{
    Super::BeginPlay();

    // Bind hit event
    CollisionComponent->OnComponentHit.AddDynamic(this, &AFBBullet::OnHit);

    // Spawn trail effect
    if (TrailEffect)
    {
        UNiagaraFunctionLibrary::SpawnSystemAttached(
            TrailEffect,
            CollisionComponent,
            NAME_None,
            FVector::ZeroVector,
            FRotator::ZeroRotator,
            FVector(1.0f),
            EAttachLocation::KeepRelativeOffset,
            true
        );
    }
}

void AFBBullet::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);
}

void AFBBullet::Initialize(float InSpeed, float InDamage)
{
    Speed = InSpeed;
    Damage = InDamage;
    
    if (ProjectileMovement)
    {
        ProjectileMovement->InitialSpeed = Speed;
        ProjectileMovement->MaxSpeed = Speed;
    }
}

void AFBBullet::OnHit(UPrimitiveComponent* HitComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit)
{
    // Don't hit the same actor multiple times
    if (HitActors.Contains(OtherActor))
    {
        return;
    }

    HitActors.Add(OtherActor);

    // Apply damage
    ApplyDamage(OtherActor, Hit);

    // Play impact effects
    PlayImpactEffects(Hit.Location, Hit.Normal);

    // Handle penetration
    if (bPenetrate && CurrentPenetrations < MaxPenetrations)
    {
        CurrentPenetrations++;
        // Reduce damage for next penetration
        Damage *= 0.7f;
        return; // Don't destroy bullet, let it continue
    }

    // Destroy bullet
    Destroy();

    // Multicast impact effects
    if (HasAuthority())
    {
        Multicast_OnImpact(Hit.Location, Hit.Normal);
    }
}

void AFBBullet::PlayImpactEffects(const FVector& ImpactLocation, const FVector& ImpactNormal)
{
    // Spawn impact effect
    if (ImpactEffect)
    {
        UNiagaraFunctionLibrary::SpawnSystemAtLocation(
            GetWorld(),
            ImpactEffect,
            ImpactLocation,
            ImpactNormal.Rotation()
        );
    }

    // Play impact sound
    if (ImpactSound)
    {
        UGameplayStatics::PlaySoundAtLocation(
            this,
            ImpactSound,
            ImpactLocation
        );
    }
}

void AFBBullet::ApplyDamage(AActor* HitActor, const FHitResult& Hit)
{
    if (HitActor && HitActor != GetOwner())
    {
        // Apply damage using Unreal's damage system
        UGameplayStatics::ApplyPointDamage(
            HitActor,
            Damage,
            (Hit.Location - GetActorLocation()).GetSafeNormal(),
            Hit,
            GetInstigatorController(),
            this,
            UDamageType::StaticClass()
        );
    }
}

void AFBBullet::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);

    DOREPLIFETIME(AFBBullet, Damage);
    DOREPLIFETIME(AFBBullet, Speed);
}

void AFBBullet::Multicast_OnImpact_Implementation(const FVector& ImpactLocation, const FVector& ImpactNormal)
{
    // Play impact effects on all clients
    if (!HasAuthority())
    {
        PlayImpactEffects(ImpactLocation, ImpactNormal);
    }
}
