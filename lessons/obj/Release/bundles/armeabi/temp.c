/* This source code was produced by mkbundle, do not edit */

#ifndef NULL
#define NULL (void *)0
#endif

typedef struct {
	const char *name;
	const unsigned char *data;
	const unsigned int size;
} MonoBundledAssembly;
void          mono_register_bundled_assemblies (const MonoBundledAssembly **assemblies);
void          register_config_for_assembly_func (const char* assembly_name, const char* config_xml);

extern const unsigned char assembly_data_lessons_dll [];
static const MonoBundledAssembly assembly_bundle_lessons_dll = {"lessons.dll", assembly_data_lessons_dll, 9728};
extern const unsigned char assembly_data_Mono_Android_dll [];
static const MonoBundledAssembly assembly_bundle_Mono_Android_dll = {"Mono.Android.dll", assembly_data_Mono_Android_dll, 630272};
extern const unsigned char assembly_data_mscorlib_dll [];
static const MonoBundledAssembly assembly_bundle_mscorlib_dll = {"mscorlib.dll", assembly_data_mscorlib_dll, 1392128};
extern const unsigned char assembly_data_System_dll [];
static const MonoBundledAssembly assembly_bundle_System_dll = {"System.dll", assembly_data_System_dll, 436224};
extern const unsigned char assembly_data_Mono_Security_dll [];
static const MonoBundledAssembly assembly_bundle_Mono_Security_dll = {"Mono.Security.dll", assembly_data_Mono_Security_dll, 173056};
extern const unsigned char assembly_data_System_Core_dll [];
static const MonoBundledAssembly assembly_bundle_System_Core_dll = {"System.Core.dll", assembly_data_System_Core_dll, 32768};
extern const unsigned char assembly_data_I18N_dll [];
static const MonoBundledAssembly assembly_bundle_I18N_dll = {"I18N.dll", assembly_data_I18N_dll, 37888};
extern const unsigned char assembly_data_I18N_Other_dll [];
static const MonoBundledAssembly assembly_bundle_I18N_Other_dll = {"I18N.Other.dll", assembly_data_I18N_Other_dll, 35840};

static const MonoBundledAssembly *bundled [] = {
	&assembly_bundle_lessons_dll,
	&assembly_bundle_Mono_Android_dll,
	&assembly_bundle_mscorlib_dll,
	&assembly_bundle_System_dll,
	&assembly_bundle_Mono_Security_dll,
	&assembly_bundle_System_Core_dll,
	&assembly_bundle_I18N_dll,
	&assembly_bundle_I18N_Other_dll,
	NULL
};

static char *image_name = "lessons.dll";

static void install_dll_config_files (void (register_config_for_assembly_func)(const char *, const char *)) {

}

static const char *config_dir = NULL;
void mono_mkbundle_init (void (register_bundled_assemblies_func)(const MonoBundledAssembly **), void (register_config_for_assembly_func)(const char *, const char *))
{
	install_dll_config_files (register_config_for_assembly_func);
	register_bundled_assemblies_func(bundled);
}
