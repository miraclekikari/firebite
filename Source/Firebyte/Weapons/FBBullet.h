#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Actor.h"
#include "Engine/EngineTypes.h"
#include "FBBullet.generated.h"

class USphereComponent;
class UProjectileMovementComponent;
class UNiagaraSystem;
class USoundBase;

UCLASS(Blueprintable, BlueprintType)
class FIREBYTE_API AFBBullet : public AActor
{
    GENERATED_BODY()

public:
    AFBBullet();

    // Components
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components")
    TObjectPtr<USphereComponent> CollisionComponent;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "Components")
    TObjectPtr<UProjectileMovementComponent> ProjectileMovement;

    // Bullet properties
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bullet")
    float Damage = 25.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bullet")
    float Speed = 3000.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bullet")
    float Lifespan = 3.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bullet")
    bool bPenetrate = false;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Bullet")
    int32 MaxPenetrations = 1;

    // Effects
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<UNiagaraSystem> TrailEffect;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<UNiagaraSystem> ImpactEffect;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Effects")
    TObjectPtr<USoundBase> ImpactSound;

protected:
    virtual void BeginPlay() override;

    // Internal variables
    int32 CurrentPenetrations = 0;
    TArray<AActor*> HitActors;

public:
    // Initialize bullet
    UFUNCTION(BlueprintCallable, Category = "Bullet")
    void Initialize(float InSpeed, float InDamage);

    // Override functions
    virtual void Tick(float DeltaTime) override;

protected:
    // Collision handling
    UFUNCTION()
    virtual void OnHit(UPrimitiveComponent* HitComp, AActor* OtherActor, UPrimitiveComponent* OtherComp, FVector NormalImpulse, const FHitResult& Hit);

    // Impact effects
    UFUNCTION(BlueprintCallable, Category = "Bullet")
    virtual void PlayImpactEffects(const FVector& ImpactLocation, const FVector& ImpactNormal);

    // Damage application
    UFUNCTION(BlueprintCallable, Category = "Bullet")
    virtual void ApplyDamage(AActor* HitActor, const FHitResult& Hit);

    // Network replication
    virtual void GetLifetimeReplicatedProps(TArray<FLifetimeProperty>& OutLifetimeProps) const override;

    UFUNCTION(NetMulticast, Reliable)
    void Multicast_OnImpact(const FVector& ImpactLocation, const FVector& ImpactNormal);
};
