#include <stdlib.h>

#include "epsilon.h"

const char *epsl_error_stack[ERROR_STACK_SIZE] = {0};
const char **epsl_error_stack_top = epsl_error_stack;
char **epsl_argv = NULL;
