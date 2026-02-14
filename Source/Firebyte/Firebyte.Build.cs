using UnrealBuildTool;

public class Firebyte : ModuleRules
{
    public Firebyte(ReadOnlyTargetRules Target) : base(Target)
    {
        PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

        PublicDependencyModuleNames.AddRange(new string[] 
        { 
            "Core", 
            "CoreUObject", 
            "Engine", 
            "InputCore",
            "EnhancedInput",
            "GameplayTags",
            "GameplayTasks",
            "NetCore",
            "UMG",
            "Slate",
            "SlateCore"
        });

        PrivateDependencyModuleNames.AddRange(new string[] 
        { 
            "SlateCore",
            "AIModule",
            "Niagara",
            "AudioMixer",
            "PhysicsCore",
            "KismetAnimationLibrary",
            "KismetMathLibrary",
            "KismetSystemLibrary"
        });

        // Enable networking support
        if (Target.bBuildEditor)
        {
            PrivateDependencyModuleNames.AddRange(new string[]
            {
                "UnrealEd",
                "ToolMenus",
                "EditorStyle"
            });
        }

        // Optimization settings
        OptimizeCode = CodeOptimization.InShippingBuildsOnly;
        
        // Enable hot reload for development
        bUseUnity = false;
    }
}
