# patch1337-to-cpp
Converts a `.1337` file to a `.cpp` runtime patcher.

Use case is for DLL Injection and automatic patching of the binary on runtime.

Uses code from https://stackoverflow.com/a/48737037 to patch on runtime, thanks!

Sample output:
```cpp
#include <vector>
#include <windows.h>
int BaseAddress = (int)GetModuleHandle(nullptr);
struct PatchAddress { int Address; unsigned char OldByte; unsigned char NewByte; };
struct Patch { const char* ModuleName; std::vector<PatchAddress> Patches;};

void PatchUChar(unsigned char* dst, unsigned char* src, int size) {
DWORD oldprotect;
VirtualProtect(dst, size, PAGE_EXECUTE_READWRITE, &oldprotect);
memcpy(dst, src, size);
VirtualProtect(dst, size, oldprotect, &oldprotect); };

Patch executable_exe = { "executable.exe", {
PatchAddress{ 126805, 87, 88 },
PatchAddress{ 126885, 7, 8 },
PatchAddress{ 126948, 200, 201 },
PatchAddress{ 126965, 183, 184 },
PatchAddress{ 126982, 166, 167 },
PatchAddress{ 126999, 149, 150 },
PatchAddress{ 127019, 129, 130 },
PatchAddress{ 127036, 112, 113 },
PatchAddress{ 127529, 131, 132 },
PatchAddress{ 127575, 85, 86 },
PatchAddress{ 127621, 39, 40 },
} };
void patch_executable_exe() {
for (PatchAddress addr : executable_exe.Patches) {
PatchUChar((unsigned char*)(BaseAddress + addr.Address), &addr.NewByte, 1); }
};
void unpatch_executable_exe() {
for (PatchAddress addr : executable_exe.Patches) {
PatchUChar((unsigned char*)(BaseAddress + addr.Address), &addr.OldByte, 1); }
};
```
