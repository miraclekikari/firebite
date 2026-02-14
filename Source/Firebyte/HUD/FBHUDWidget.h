#pragma once

#include "CoreMinimal.h"
#include "Blueprint/UserWidget.h"
#include "FBHUDWidget.generated.h"

class UProgressBar;
class UTextBlock;
class UImage;
class UCanvasPanel;
class UOverlay;
class UBorder;

USTRUCT(BlueprintType)
struct FPlayerStats
{
    GENERATED_BODY()

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float HealthPercentage = 1.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float EnergyPercentage = 1.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    float XPPercentage = 0.0f;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    int32 Level = 1;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    FString AmmoText = TEXT("30/30 | 90");

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Stats")
    FString WeaponName = TEXT("Assault Rifle");
};

UCLASS(Blueprintable, BlueprintType)
class FIREBYTE_API UFBHUDWidget : public UUserWidget
{
    GENERATED_BODY()

public:
    UFBHUDWidget(const FObjectInitializer& ObjectInitializer);

    // HUD Components
    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UCanvasPanel> MainCanvas;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UOverlay> HealthOverlay;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UProgressBar> HealthBar;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UTextBlock> HealthText;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UOverlay> EnergyOverlay;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UProgressBar> EnergyBar;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UTextBlock> EnergyText;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UTextBlock> LevelText;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UTextBlock> XPText;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UTextBlock> AmmoText;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UTextBlock> WeaponNameText;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UImage> Crosshair;

    UPROPERTY(meta = (BindWidget))
    TObjectPtr<UBorder> DamageIndicator;

    // HUD Animation
    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animation")
    TObjectPtr<UWidgetAnimation> DamageFlashAnimation;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animation")
    TObjectPtr<UWidgetAnimation> LowHealthPulseAnimation;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animation")
    TObjectPtr<UWidgetAnimation> LevelUpAnimation;

    UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Animation")
    TObjectPtr<UWidgetAnimation> ReloadAnimation;

protected:
    virtual void NativeConstruct() override;
    virtual void NativeTick(const FGeometry& MyGeometry, float InDeltaTime) override;

    // Player stats
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "HUD")
    FPlayerStats PlayerStats;

    // Animation state
    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "HUD")
    bool bIsLowHealth = false;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "HUD")
    bool bIsReloading = false;

    UPROPERTY(VisibleAnywhere, BlueprintReadOnly, Category = "HUD")
    float DamageIndicatorAlpha = 0.0f;

    // Timers
    UPROPERTY()
    FTimerHandle DamageIndicatorTimer;

    UPROPERTY()
    FTimerHandle ReloadTimer;

public:
    // Update functions
    UFUNCTION(BlueprintCallable, Category = "HUD")
    virtual void UpdatePlayerStats(float HealthPercent, float EnergyPercent, float XPPercent, int32 CurrentLevel);

    UFUNCTION(BlueprintCallable, Category = "HUD")
    virtual void UpdateAmmo(const FString& AmmoString);

    UFUNCTION(BlueprintCallable, Category = "HUD")
    virtual void UpdateWeaponName(const FString& WeaponName);

    UFUNCTION(BlueprintCallable, Category = "HUD")
    virtual void ShowDamageIndicator();

    UFUNCTION(BlueprintCallable, Category = "HUD")
    virtual void ShowReloadAnimation();

    UFUNCTION(BlueprintCallable, Category = "HUD")
    virtual void ShowLevelUpAnimation();

    UFUNCTION(BlueprintCallable, Category = "HUD")
    virtual void SetCrosshairVisibility(bool bVisible);

    UFUNCTION(BlueprintCallable, Category = "HUD")
    virtual void SetCrosshairColor(FLinearColor Color);

    // Utility functions
    UFUNCTION(BlueprintPure, Category = "HUD")
    FORCEINLINE FPlayerStats GetPlayerStats() const { return PlayerStats; }

    UFUNCTION(BlueprintPure, Category = "HUD")
    FORCEINLINE bool IsLowHealth() const { return bIsLowHealth; }

protected:
    // Internal functions
    virtual void UpdateHealthDisplay();
    virtual void UpdateEnergyDisplay();
    virtual void UpdateXPDisplay();
    virtual void UpdateAmmoDisplay();
    virtual void UpdateLowHealthState();
    virtual void UpdateDamageIndicator(float DeltaTime);

    // Event handlers
    UFUNCTION()
    virtual void OnDamageIndicatorTimer();

    UFUNCTION()
    virtual void OnReloadTimer();

    // Styling functions
    virtual void ApplyCyberpunkStyling();
    virtual void UpdateHealthBarColor();
    virtual void UpdateEnergyBarColor();

    // Widget binding
    virtual bool InitializeWidgetBindings();

    // Network replication
    UFUNCTION(Client, Reliable)
    void Client_UpdatePlayerStats(const FPlayerStats& NewStats);

    UFUNCTION(Client, Reliable)
    void Client_ShowDamageIndicator();

    UFUNCTION(Client, Reliable)
    void Client_ShowLevelUpAnimation();
};
