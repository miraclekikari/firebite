#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Engine/EngineTypes.h"
#include "FBWeapon.generated.h"

class USkeletalMeshComponent;
class USceneComponent;
class UNiagaraSystem;
class USoundBase;
class UAnimMontage;
class AFBBullet;

UENUM(BlueprintType)
enum class EWeaponState : uint8
{
    Idle,
    Firing,
    Reloading,
    Equipping,
    Unequipping
};

UENUM(BlueprintType)
enum class EWeaponType : uint8
{
    AssaultRifle,
    Shotgun,
    SniperRifle,
    Pistol,
    PlasmaRifle,
    RocketLauncher
};

USTRUCT(BlueprintType)
struct FWeaponStats
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float BaseDamage = 25.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float FireRate = 600.0f; // Rounds per minute

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float Range = 5000.0f; // in cm

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    int32 MagazineSize = 30;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    int32 ReserveAmmo = 90;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float ReloadTime = 2.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float BulletSpeed = 3000.0f; // cm/s

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float SpreadAngle = 1.0f; // degrees

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    int32 BulletsPerShot = 1;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    bool bAutomaticFire = true;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    bool bHasZoom = false;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float ZoomFOV = 45.0f;
};

UCLASS(Abstract, Blueprintable, BlueprintType)
class FIREBYTE_API AFBWeapon : public AActor
{
    GENERATED_BODY()

public:
    AFBWeapon();

    // Weapon components
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components")
    TObjectPtr<USkeletalMeshComponent> WeaponMesh;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components")
    TObjectPtr<USceneComponent> MuzzleLocation;

    // Weapon configuration
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
    FWeaponStats WeaponStats;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
    EWeaponType WeaponType;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Weapon")
    TSubclassOf<AFBBullet> BulletClass;

    // Visual effects
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<UNiagaraSystem> MuzzleFlashEffect;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<UNiagaraSystem> ImpactEffect;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<USoundBase> FireSound;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<USoundBase> ReloadSound;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<USoundBase> EmptySound;

    // Animations
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animations")
    TObjectPtr<UAnimMontage> FireAnimation;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animations")
    TObjectPtr<UAnimMontage> ReloadAnimation;

protected:
    virtual void BeginPlay() override;
    virtual void Tick(float DeltaTime) override;

    // Weapon state
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Weapon")
    EWeaponState CurrentState = EWeaponState::Idle;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Weapon")
    int32 CurrentAmmo = 30;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Weapon")
    int32 CurrentReserveAmmo = 90;

    // Timers
    UPROPERTY()
    FTimerHandle FireTimer;

    UPROPERTY()
    FTimerHandle ReloadTimer;

    // Internal variables
    bool bWantsToFire = false;
    float LastFireTime = 0.0f;

public:
    // Weapon actions
    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void StartFire();

    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void StopFire();

    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void Reload();

    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void Equip();

    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void Unequip();

    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void AddAmmo(int32 Amount);

    // Weapon functionality
    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void Fire();

    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void ProcessHit(const FHitResult& Hit);

    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void PlayMuzzleEffects();

    UFUNCTION(BlueprintCallable, Category = "Weapon")
    virtual void PlayImpactEffects(const FVector& ImpactLocation, const FVector& ImpactNormal);

    // Network replication
    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

    // Getters
    UFUNCTION(BlueprintPure, Category = "Weapon")
    FORCEINLINE EWeaponState GetCurrentState() const { return CurrentState; }

    UFUNCTION(BlueprintPure, Category = "Weapon")
    FORCEINLINE int32 GetCurrentAmmo() const { return CurrentAmmo; }

    UFUNCTION(BlueprintPure, Category = "Weapon")
    FORCEINLINE int32 GetCurrentReserveAmmo() const { return CurrentReserveAmmo; }

    UFUNCTION(BlueprintPure, Category = "Weapon")
    FORCEINLINE float GetAmmoPercentage() const { return (float)CurrentAmmo / WeaponStats.MagazineSize; }

    UFUNCTION(BlueprintPure, Category = "Weapon")
    FORCEINLINE bool CanFire() const 
    { 
        return CurrentState == EWeaponState::Idle && CurrentAmmo > 0; 
    }

    UFUNCTION(BlueprintPure, Category = "Weapon")
    FORCEINLINE bool CanReload() const 
    { 
        return CurrentState == EWeaponState::Idle && CurrentAmmo < WeaponStats.MagazineSize && CurrentReserveAmmo > 0; 
    }

    UFUNCTION(BlueprintPure, Category = "Weapon")
    FORCEINLINE bool IsEmpty() const 
    { 
        return CurrentAmmo == 0; 
    }

protected:
    // Internal functions
    UFUNCTION()
    virtual void OnFireTimer();

    UFUNCTION()
    virtual void OnReloadTimer();

    UFUNCTION()
    virtual void OnRep_CurrentAmmo();

    UFUNCTION()
    virtual void OnRep_CurrentState();

    // Network functions
    UFUNCTION(Server, Reliable, WithValidation)
    void Server_StartFire();

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_StopFire();

    UFUNCTION(Server, Reliable, WithValidation)
    void Server_Reload();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_PlayMuzzleEffects();

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_PlayImpactEffects(const FVector& ImpactLocation, const FVector& ImpactNormal);
};
