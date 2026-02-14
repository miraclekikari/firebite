#include "FBHUDWidget.h"
#include "Components/ProgressBar.h"
#include "Components/TextBlock.h"
#include "Components/Image.h"
#include "Components/CanvasPanel.h"
#include "Components/Overlay.h"
#include "Components/Border.h"
import "Animation/WidgetAnimation.h"
#include "TimerManager.h"
#include "Engine/World.h"
#include "Kismet/KismetMathLibrary.h"

UFBHUDWidget::UFBHUDWidget(const FObjectInitializer& ObjectInitializer)
    : Super(ObjectInitializer)
{
    // Initialize default values
    PlayerStats.HealthPercentage = 1.0f;
    PlayerStats.EnergyPercentage = 1.0f;
    PlayerStats.XPPercentage = 0.0f;
    PlayerStats.Level = 1;
    PlayerStats.AmmoText = TEXT("30/30 | 90");
    PlayerStats.WeaponName = TEXT("Assault Rifle");
}

void UFBHUDWidget::NativeConstruct()
{
    Super::NativeConstruct();

    // Initialize widget bindings
    InitializeWidgetBindings();

    // Apply cyberpunk styling
    ApplyCyberpunkStyling();

    // Set initial values
    UpdateHealthDisplay();
    UpdateEnergyDisplay();
    UpdateXPDisplay();
    UpdateAmmoDisplay();

    // Set crosshair visibility
    SetCrosshairVisibility(true);
}

void UFBHUDWidget::NativeTick(const FGeometry& MyGeometry, float InDeltaTime)
{
    Super::NativeTick(MyGeometry, InDeltaTime);

    // Update damage indicator
    UpdateDamageIndicator(InDeltaTime);

    // Update low health state
    UpdateLowHealthState();
}

void UFBHUDWidget::UpdatePlayerStats(float HealthPercent, float EnergyPercent, float XPPercent, int32 CurrentLevel)
{
    PlayerStats.HealthPercentage = FMath::Clamp(HealthPercent, 0.0f, 1.0f);
    PlayerStats.EnergyPercentage = FMath::Clamp(EnergyPercent, 0.0f, 1.0f);
    PlayerStats.XPPercentage = FMath::Clamp(XPPercent, 0.0f, 1.0f);
    PlayerStats.Level = CurrentLevel;

    UpdateHealthDisplay();
    UpdateEnergyDisplay();
    UpdateXPDisplay();
}

void UFBHUDWidget::UpdateAmmo(const FString& AmmoString)
{
    PlayerStats.AmmoText = AmmoString;
    UpdateAmmoDisplay();
}

void UFBHUDWidget::UpdateWeaponName(const FString& WeaponName)
{
    PlayerStats.WeaponName = WeaponName;
    
    if (WeaponNameText)
    {
        WeaponNameText->SetText(FText::FromString(WeaponName));
    }
}

void UFBHUDWidget::ShowDamageIndicator()
{
    if (DamageIndicator)
    {
        DamageIndicatorAlpha = 1.0f;
        DamageIndicator->SetRenderOpacity(DamageIndicatorAlpha);

        // Play damage flash animation
        if (DamageFlashAnimation)
        {
            PlayAnimation(DamageFlashAnimation);
        }

        // Start fade timer
        GetWorld()->GetTimerManager().SetTimer(
            DamageIndicatorTimer,
            this,
            &UFBHUDWidget::OnDamageIndicatorTimer,
            0.1f,
            true
        );
    }
}

void UFBHUDWidget::ShowReloadAnimation()
{
    bIsReloading = true;

    if (ReloadAnimation)
    {
        PlayAnimation(ReloadAnimation);
    }

    // Set reload timer
    GetWorld()->GetTimerManager().SetTimer(
        ReloadTimer,
        this,
        &UFBHUDWidget::OnReloadTimer,
        2.0f,
        false
    );
}

void UFBHUDWidget::ShowLevelUpAnimation()
{
    if (LevelUpAnimation)
    {
        PlayAnimation(LevelUpAnimation);
    }
}

void UFBHUDWidget::SetCrosshairVisibility(bool bVisible)
{
    if (Crosshair)
    {
        Crosshair->SetVisibility(bVisible);
    }
}

void UFBHUDWidget::SetCrosshairColor(FLinearColor Color)
{
    if (Crosshair)
    {
        Crosshair->SetColorAndOpacity(Color);
    }
}

void UFBHUDWidget::UpdateHealthDisplay()
{
    if (HealthBar)
    {
        HealthBar->SetPercent(PlayerStats.HealthPercentage);
        UpdateHealthBarColor();
    }

    if (HealthText)
    {
        int32 CurrentHealth = FMath::RoundToInt(PlayerStats.HealthPercentage * 100);
        HealthText->SetText(FText::FromString(FString::Printf(TEXT("%d/100"), CurrentHealth)));
    }
}

void UFBHUDWidget::UpdateEnergyDisplay()
{
    if (EnergyBar)
    {
        EnergyBar->SetPercent(PlayerStats.EnergyPercentage);
        UpdateEnergyBarColor();
    }

    if (EnergyText)
    {
        int32 CurrentEnergy = FMath::RoundToInt(PlayerStats.EnergyPercentage * 100);
        EnergyText->SetText(FText::FromString(FString::Printf(TEXT("%d/100"), CurrentEnergy)));
    }
}

void UFBHUDWidget::UpdateXPDisplay()
{
    if (LevelText)
    {
        LevelText->SetText(FText::FromString(FString::Printf(TEXT("LEVEL %d"), PlayerStats.Level)));
    }

    if (XPText)
    {
        int32 CurrentXP = FMath::RoundToInt(PlayerStats.XPPercentage * 100);
        XPText->SetText(FText::FromString(FString::Printf(TEXT("XP: %d%%"), CurrentXP)));
    }
}

void UFBHUDWidget::UpdateAmmoDisplay()
{
    if (AmmoText)
    {
        AmmoText->SetText(FText::FromString(PlayerStats.AmmoText));
    }
}

void UFBHUDWidget::UpdateLowHealthState()
{
    bool bNewLowHealth = PlayerStats.HealthPercentage <= 0.3f;

    if (bNewLowHealth != bIsLowHealth)
    {
        bIsLowHealth = bNewLowHealth;

        if (bIsLowHealth)
        {
            // Start low health pulse animation
            if (LowHealthPulseAnimation)
            {
                PlayAnimation(LowHealthPulseAnimation);
            }
        }
        else
        {
            // Stop low health pulse animation
            if (LowHealthPulseAnimation)
            {
                StopAnimation(LowHealthPulseAnimation);
            }
        }
    }
}

void UFBHUDWidget::UpdateDamageIndicator(float DeltaTime)
{
    if (DamageIndicatorAlpha > 0.0f)
    {
        DamageIndicatorAlpha = FMath::Max(0.0f, DamageIndicatorAlpha - DeltaTime * 2.0f);
        
        if (DamageIndicator)
        {
            DamageIndicator->SetRenderOpacity(DamageIndicatorAlpha);
        }
    }
}

void UFBHUDWidget::OnDamageIndicatorTimer()
{
    // Fade damage indicator
    DamageIndicatorAlpha = FMath::Max(0.0f, DamageIndicatorAlpha - 0.1f);
    
    if (DamageIndicatorAlpha <= 0.0f)
    {
        GetWorld()->GetTimerManager().ClearTimer(DamageIndicatorTimer);
    }
}

void UFBHUDWidget::OnReloadTimer()
{
    bIsReloading = false;
}

void UFBHUDWidget::ApplyCyberpunkStyling()
{
    // Apply cyberpunk colors and styling
    if (HealthBar)
    {
        HealthBar->SetFillColorAndOpacity(FLinearColor(1.0f, 0.1f, 0.1f, 0.8f)); // Red
    }

    if (EnergyBar)
    {
        EnergyBar->SetFillColorAndOpacity(FLinearColor(0.1f, 0.8f, 1.0f, 0.8f)); // Cyan
    }

    if (Crosshair)
    {
        Crosshair->SetColorAndOpacity(FLinearColor(0.0f, 1.0f, 1.0f, 0.8f)); // Cyan
    }

    // Set text colors
    FLinearColor CyberpunkTextColor = FLinearColor(0.0f, 1.0f, 0.8f, 1.0f); // Neon green

    if (HealthText)
    {
        HealthText->SetColorAndOpacity(CyberpunkTextColor);
    }

    if (EnergyText)
    {
        EnergyText->SetColorAndOpacity(CyberpunkTextColor);
    }

    if (LevelText)
    {
        LevelText->SetColorAndOpacity(FLinearColor(1.0f, 0.8f, 0.0f, 1.0f)); // Gold
    }

    if (XPText)
    {
        XPText->SetColorAndOpacity(CyberpunkTextColor);
    }

    if (AmmoText)
    {
        AmmoText->SetColorAndOpacity(CyberpunkTextColor);
    }

    if (WeaponNameText)
    {
        WeaponNameText->SetColorAndOpacity(CyberpunkTextColor);
    }
}

void UFBHUDWidget::UpdateHealthBarColor()
{
    if (!HealthBar)
        return;

    // Change color based on health percentage
    FLinearColor HealthColor;
    
    if (PlayerStats.HealthPercentage > 0.6f)
    {
        HealthColor = FLinearColor(0.1f, 1.0f, 0.1f, 0.8f); // Green
    }
    else if (PlayerStats.HealthPercentage > 0.3f)
    {
        HealthColor = FLinearColor(1.0f, 1.0f, 0.1f, 0.8f); // Yellow
    }
    else
    {
        HealthColor = FLinearColor(1.0f, 0.1f, 0.1f, 0.8f); // Red
    }

    HealthBar->SetFillColorAndOpacity(HealthColor);
}

void UFBHUDWidget::UpdateEnergyBarColor()
{
    if (!EnergyBar)
        return;

    // Energy bar always cyan with slight variation based on percentage
    float Alpha = 0.5f + (PlayerStats.EnergyPercentage * 0.5f);
    FLinearColor EnergyColor = FLinearColor(0.1f, 0.8f, 1.0f, Alpha);
    EnergyBar->SetFillColorAndOpacity(EnergyColor);
}

bool UFBHUDWidget::InitializeWidgetBindings()
{
    // This would be automatically handled by the UMG editor
    // but we can add fallback logic here if needed
    return true;
}

void UFBHUDWidget::Client_UpdatePlayerStats_Implementation(const FPlayerStats& NewStats)
{
    PlayerStats = NewStats;
    UpdateHealthDisplay();
    UpdateEnergyDisplay();
    UpdateXPDisplay();
    UpdateAmmoDisplay();
}

void UFBHUDWidget::Client_ShowDamageIndicator_Implementation()
{
    ShowDamageIndicator();
}

void UFBHUDWidget::Client_ShowLevelUpAnimation_Implementation()
{
    ShowLevelUpAnimation();
}
