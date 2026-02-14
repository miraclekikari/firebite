#include "FBWeapon.h"
#include "FBBullet.h"
#include "Components/SkeletalMeshComponent.h"
#include "Components/SceneComponent.h"
#include "NiagaraFunctionLibrary.h"
#include "Kismet/GameplayStatics.h"
#include "Kismet/KismetSystemLibrary.h"
#include "Net/UnrealNetwork.h"
#include "TimerManager.h"
#include "Engine/World.h"

AFBWeapon::AFBWeapon()
{
    PrimaryActorTick.bCanEverTick = true;
    bReplicates = true;
    SetReplicateMovement(true);

    // Create components
    WeaponMesh = CreateDefaultSubobject<USkeletalMeshComponent>(TEXT("WeaponMesh"));
    RootComponent = WeaponMesh;
    WeaponMesh->SetCollisionEnabled(ECollisionEnabled::NoCollision);
    WeaponMesh->SetIsReplicated(true);

    MuzzleLocation = CreateDefaultSubobject<USceneComponent>(TEXT("MuzzleLocation"));
    MuzzleLocation->SetupAttachment(WeaponMesh);
    MuzzleLocation->SetRelativeLocation(FVector(50.0f, 0.0f, 0.0f));

    // Initialize weapon stats
    CurrentAmmo = WeaponStats.MagazineSize;
    CurrentReserveAmmo = WeaponStats.ReserveAmmo;
}

void AFBWeapon::BeginPlay()
{
    Super::BeginPlay();

    // Initialize ammo
    CurrentAmmo = WeaponStats.MagazineSize;
    CurrentReserveAmmo = WeaponStats.ReserveAmmo;
}

void AFBWeapon::Tick(float DeltaTime)
{
    Super::Tick(DeltaTime);
}

void AFBWeapon::StartFire()
{
    if (!CanFire())
    {
        if (IsEmpty())
        {
            // Play empty sound
            if (EmptySound)
            {
                UGameplayStatics::PlaySoundAtLocation(this, EmptySound, GetActorLocation());
            }
        }
        return;
    }

    bWantsToFire = true;

    if (WeaponStats.bAutomaticFire)
    {
        // Start automatic fire
        float FireInterval = 60.0f / WeaponStats.FireRate;
        GetWorldTimerManager().SetTimer(FireTimer, this, &AFBWeapon::OnFireTimer, FireInterval, true);
    }
    else
    {
        // Single shot
        Fire();
    }

    if (HasAuthority())
    {
        Server_StartFire();
    }
}

void AFBWeapon::StopFire()
{
    bWantsToFire = false;

    if (WeaponStats.bAutomaticFire)
    {
        GetWorldTimerManager().ClearTimer(FireTimer);
    }

    if (HasAuthority())
    {
        Server_StopFire();
    }
}

void AFBWeapon::Reload()
{
    if (!CanReload())
        return;

    CurrentState = EWeaponState::Reloading;
    GetWorldTimerManager().SetTimer(ReloadTimer, this, &AFBWeapon::OnReloadTimer, WeaponStats.ReloadTime, false);

    // Play reload sound
    if (ReloadSound)
    {
        UGameplayStatics::PlaySoundAtLocation(this, ReloadSound, GetActorLocation());
    }

    if (HasAuthority())
    {
        Server_Reload();
    }
}

void AFBWeapon::Equip()
{
    CurrentState = EWeaponState::Equipping;
    // Play equip animation/sound
    CurrentState = EWeaponState::Idle;
}

void AFBWeapon::Unequip()
{
    CurrentState = EWeaponState::Unequipping;
    StopFire();
    // Play unequip animation/sound
}

void AFBWeapon::AddAmmo(int32 Amount)
{
    if (HasAuthority())
    {
        CurrentReserveAmmo = FMath::Min(WeaponStats.ReserveAmmo, CurrentReserveAmmo + Amount);
        OnRep_CurrentReserveAmmo();
    }
}

void AFBWeapon::Fire()
{
    if (!CanFire())
        return;

    CurrentState = EWeaponState::Firing;
    CurrentAmmo--;

    // Calculate fire direction with spread
    FVector StartLocation = MuzzleLocation->GetComponentLocation();
    FRotator FireRotation = MuzzleLocation->GetComponentRotation();

    // Add spread
    float SpreadRad = FMath::DegreesToRadians(WeaponStats.SpreadAngle);
    FireRotation.Yaw += FMath::RandRange(-SpreadRad, SpreadRad);
    FireRotation.Pitch += FMath::RandRange(-SpreadRad, SpreadRad);

    FVector EndLocation = StartLocation + FireRotation.Vector() * WeaponStats.Range;

    // Perform line trace
    FHitResult HitResult;
    FCollisionQueryParams QueryParams;
    QueryParams.AddIgnoredActor(this);
    QueryParams.AddIgnoredActor(GetOwner());

    bool bHit = GetWorld()->LineTraceSingleByChannel(
        HitResult,
        StartLocation,
        EndLocation,
        ECC_Visibility,
        QueryParams
    );

    // Spawn bullet or process hit
    if (BulletClass)
    {
        FActorSpawnParameters SpawnParams;
        SpawnParams.Owner = GetOwner();
        SpawnParams.Instigator = GetInstigator();

        AFBBullet* Bullet = GetWorld()->SpawnActor<AFBBullet>(BulletClass, StartLocation, FireRotation, SpawnParams);
        if (Bullet)
        {
            Bullet->Initialize(WeaponStats.BulletSpeed, WeaponStats.BaseDamage);
        }
    }

    if (bHit)
    {
        ProcessHit(HitResult);
        PlayImpactEffects(HitResult.Location, HitResult.Normal);
    }

    // Play muzzle effects
    PlayMuzzleEffects();

    // Play fire sound
    if (FireSound)
    {
        UGameplayStatics::PlaySoundAtLocation(this, FireSound, GetActorLocation());
    }

    CurrentState = EWeaponState::Idle;

    // Auto-reload if empty
    if (CurrentAmmo == 0 && CurrentReserveAmmo > 0)
    {
        Reload();
    }

    if (HasAuthority())
    {
        OnRep_CurrentAmmo();
    }
}

void AFBWeapon::ProcessHit(const FHitResult& Hit)
{
    if (Hit.GetActor() && Hit.GetActor()->GetClass()->ImplementsInterface(UFBDamageInterface::StaticClass()))
    {
        // Apply damage to hit actor
        float AppliedDamage = WeaponStats.BaseDamage;
        IFBDamageInterface::Execute_TakeDamage(Hit.GetActor(), AppliedDamage, GetInstigator(), this, nullptr);
    }

    // Spawn impact decal if needed
    // This would be implemented with decal system
}

void AFBWeapon::PlayMuzzleEffects()
{
    if (MuzzleFlashEffect)
    {
        UNiagaraFunctionLibrary::SpawnSystemAttached(
            MuzzleFlashEffect,
            MuzzleLocation,
            NAME_None,
            FVector::ZeroVector,
            FRotator::ZeroRotator,
            FVector(1.0f),
            EAttachLocation::KeepRelativeOffset,
            true
        );
    }

    if (HasAuthority())
    {
        Multicast_PlayMuzzleEffects();
    }
}

void AFBWeapon::PlayImpactEffects(const FVector& ImpactLocation, const FVector& ImpactNormal)
{
    if (ImpactEffect)
    {
        UNiagaraFunctionLibrary::SpawnSystemAtLocation(
            GetWorld(),
            ImpactEffect,
            ImpactLocation,
            ImpactNormal.Rotation()
        );
    }

    if (HasAuthority())
    {
        Multicast_PlayImpactEffects(ImpactLocation, ImpactNormal);
    }
}

void AFBWeapon::GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const
{
    Super::GetLifetimeReplicatedProps(OutLifetimeProps);

    DOREPLIFETIME(AFBWeapon, CurrentState);
    DOREPLIFETIME(AFBWeapon, CurrentAmmo);
    DOREPLIFETIME(AFBWeapon, CurrentReserveAmmo);
}

void AFBWeapon::OnFireTimer()
{
    if (bWantsToFire && CanFire())
    {
        Fire();
    }
    else
    {
        GetWorldTimerManager().ClearTimer(FireTimer);
    }
}

void AFBWeapon::OnReloadTimer()
{
    // Calculate ammo to reload
    int32 AmmoNeeded = WeaponStats.MagazineSize - CurrentAmmo;
    int32 AmmoToReload = FMath::Min(AmmoNeeded, CurrentReserveAmmo);

    CurrentAmmo += AmmoToReload;
    CurrentReserveAmmo -= AmmoToReload;

    CurrentState = EWeaponState::Idle;

    if (HasAuthority())
    {
        OnRep_CurrentAmmo();
        OnRep_CurrentReserveAmmo();
    }
}

void AFBWeapon::OnRep_CurrentAmmo()
{
    // Update UI or other client-side effects
}

void AFBWeapon::OnRep_CurrentState()
{
    // Update weapon state on clients
}

void AFBWeapon::Server_StartFire_Implementation()
{
    StartFire();
}

bool AFBWeapon::Server_StartFire_Validate()
{
    return true;
}

void AFBWeapon::Server_StopFire_Implementation()
{
    StopFire();
}

bool AFBWeapon::Server_StopFire_Validate()
{
    return true;
}

void AFBWeapon::Server_Reload_Implementation()
{
    Reload();
}

bool AFBWeapon::Server_Reload_Validate()
{
    return true;
}

void AFBWeapon::Multicast_PlayMuzzleEffects_Implementation()
{
    if (MuzzleFlashEffect && !HasAuthority())
    {
        UNiagaraFunctionLibrary::SpawnSystemAttached(
            MuzzleFlashEffect,
            MuzzleLocation,
            NAME_None,
            FVector::ZeroVector,
            FRotator::ZeroRotator,
            FVector(1.0f),
            EAttachLocation::KeepRelativeOffset,
            true
        );
    }
}

void AFBWeapon::Multicast_PlayImpactEffects_Implementation(const FVector& ImpactLocation, const FVector& ImpactNormal)
{
    if (ImpactEffect && !HasAuthority())
    {
        UNiagaraFunctionLibrary::SpawnSystemAtLocation(
            GetWorld(),
            ImpactEffect,
            ImpactLocation,
            ImpactNormal.Rotation()
        );
    }
}
