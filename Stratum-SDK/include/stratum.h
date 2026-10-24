#pragma once
#include <stdint.h>
#include <stddef.h>
#ifdef _WIN32
#define STRATUM_EXPORT __declspec(dllexport)
#define STRATUM_CALL __cdecl
#else
#define STRATUM_EXPORT
#define STRATUM_CALL
#endif
#ifdef __cplusplus
extern "C" {
#endif
#define STRATUM_NATIVE_ABI 1u
#define STRATUM_RGBA(r,g,b,a) ((uint32_t)(r)|((uint32_t)(g)<<8)|((uint32_t)(b)<<16)|((uint32_t)(a)<<24))
typedef struct StratumHost StratumHost;
typedef struct StratumFrame { uint32_t struct_size; float delta_seconds, width, height; } StratumFrame;
typedef struct StratumSetting {
    uint32_t struct_size;
    const char *key, *kind, *group, *label, *description, *default_value;
    double minimum, maximum, step;
    const char *choices; /* Choice values separated by |. Kinds: toggle, slider, choice, keybind. */
} StratumSetting;
typedef struct StratumPlugin {
    uint32_t struct_size, abi_version;
    void *user;
    int32_t (STRATUM_CALL *on_load)(void *user);
    int32_t (STRATUM_CALL *on_unload)(void *user); /* 0: stopped, otherwise restart required. */
    void (STRATUM_CALL *on_tick)(void *user, float delta_seconds);
    void (STRATUM_CALL *on_render)(void *user, const StratumFrame *frame);
    void (STRATUM_CALL *on_setting_changed)(void *user, const char *key, const char *value);
} StratumPlugin;
struct StratumHost {
    uint32_t struct_size, abi_version;
    void *context;
    void (STRATUM_CALL *log)(const StratumHost*, uint32_t level, const char *message); /* 0 Info, 1 Warning, 2 Error */
    int32_t (STRATUM_CALL *define_setting)(const StratumHost*, const StratumSetting*);
    uint32_t (STRATUM_CALL *get_setting)(const StratumHost*, const char *key, char *buffer, uint32_t capacity);
    const wchar_t* (STRATUM_CALL *config_directory)(const StratumHost*);
    const wchar_t* (STRATUM_CALL *mod_directory)(const StratumHost*);
    void (STRATUM_CALL *draw_text)(const StratumHost*, float x, float y, uint32_t rgba, const char *text);
    void (STRATUM_CALL *draw_rect)(const StratumHost*, float x, float y, float width, float height, uint32_t rgba);
};
typedef int32_t (STRATUM_CALL *StratumModMain)(const StratumHost*, StratumPlugin*);
/* Export: int32_t STRATUM_CALL stratum_mod_main(const StratumHost*, StratumPlugin*).
   Never throw across this C ABI. Lifecycle/settings/tick run on one worker thread;
   render runs on the game's D3D11 Present thread. These are NOT Unity main-thread callbacks.
   Unload must stop owned hooks/threads/resources. DLL/static state remains mapped until game exit. */
#ifdef __cplusplus
}
#endif
