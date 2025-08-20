#include <stdint.h>

#include "builtins.h"

extern int32_t epsl_main(void);

int main(int argc, char *argv[]) {
    epsl_argv = argv;
    return (int32_t)epsl_main();
}
