#include "Firebyte.h"

#define LOCTEXT_NAMESPACE "FFirebyteModule"

void FFirebyteModule::StartupModule()
{
    // This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
    
    UE_LOG(LogTemp, Log, TEXT("Firebyte module started!"));
}

void FFirebyteModule::ShutdownModule()
{
    // This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
    // we call this function before unloading the module.
    
    UE_LOG(LogTemp, Log, TEXT("Firebyte module shutdown!"));
}

#undef LOCTEXT_NAMESPACE

IMPLEMENT_MODULE(FFirebyteModule, Firebyte)
