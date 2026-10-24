#include "stratum.h"
#include <cstdio>
#include <cstdlib>
#include <cstring>
namespace {
const StratumHost* host{};bool firstTick=false,firstRender=false;double elapsed=0;
const char* value(const char* key,char* out,uint32_t n){if(!host->get_setting(host,key,out,n))out[0]=0;return out;}
int32_t STRATUM_CALL load(void*){
    const StratumSetting settings[]={
        {sizeof(StratumSetting),"hud","toggle","Общее","Показывать HUD","Нативная отрисовка в игровом окне","true",0,0,0,nullptr},
        {sizeof(StratumSetting),"interval","slider","Логи","Интервал записи","Секунды между сообщениями","10",1,60,1,nullptr},
        {sizeof(StratumSetting),"style","choice","Общее","Стиль","Цвет панели","Carbon",0,0,0,"Carbon|Orange"},
        {sizeof(StratumSetting),"hotkey","keybind","Управление","Клавиша действия","Пример настройки клавиши Windows","119",0,0,0,nullptr}};
    for(auto& s:settings)if(host->define_setting(host,&s))return 1;
    host->log(host,0,"Hello Native: C++ on_load, no CLR or managed assemblies");return 0;
}
int32_t STRATUM_CALL unload(void*){host->log(host,0,"Hello Native: on_unload completed");return 0;}
void STRATUM_CALL tick(void*,float dt){
    if(!firstTick){host->log(host,0,"First native tick");firstTick=true;}
    elapsed+=dt;char text[128];double interval=std::atof(value("interval",text,sizeof text));
    if(interval>0&&elapsed>=interval){elapsed=0;host->log(host,0,"Native tick active");}
}
void STRATUM_CALL render(void*,const StratumFrame* f){
    if(!firstRender){host->log(host,0,"First native render callback");firstRender=true;}
    char text[128];if(std::strcmp(value("hud",text,sizeof text),"true"))return;
    bool orange=std::strcmp(value("style",text,sizeof text),"Orange")==0;
    host->draw_rect(host,24,24,310,68,orange?STRATUM_RGBA(85,47,24,235):STRATUM_RGBA(25,30,32,235));
    host->draw_text(host,40,35,STRATUM_RGBA(255,157,81,255),"STRATUM Native / Hello C++");
    char line[128];std::snprintf(line,sizeof line,"Native ABI 1 / %.0f x %.0f",f->width,f->height);host->draw_text(host,40,62,STRATUM_RGBA(220,225,230,255),line);
}
void STRATUM_CALL changed(void*,const char* key,const char*){char line[256];std::snprintf(line,sizeof line,"Native setting changed: %s",key);host->log(host,0,line);}
}
extern "C" STRATUM_EXPORT int32_t STRATUM_CALL stratum_mod_main(const StratumHost* api,StratumPlugin* out){
    if(!api||!out||api->abi_version!=STRATUM_NATIVE_ABI||api->struct_size<sizeof(StratumHost)||out->struct_size<sizeof(StratumPlugin))return 1;
    host=api;*out={sizeof(StratumPlugin),STRATUM_NATIVE_ABI,nullptr,load,unload,tick,render,changed};return 0;
}
