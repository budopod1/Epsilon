#include "builtins.h"

const char *epsl_error_stack[ERROR_STACK_SIZE] = {0};
const char **epsl_error_stack_top = epsl_error_stack;
