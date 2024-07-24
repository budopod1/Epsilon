#include <cstdint>

struct CPPStruct {
    uint64_t refCounter;
    int32_t a;
    float b;
};

int cpp_func(CPPStruct *struct_) {
    return struct_->a;
}
