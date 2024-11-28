#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <string.h>
#include <stdbool.h>
#include <inttypes.h>
#include <clang-c/Index.h>
#include <stdnoreturn.h>

// clang -Xclang -ast-dump src.h

enum EPSLBuiltinType_ {
    EPSLType_Bool,
    EPSLType_Byte,
    EPSLType_W,
    EPSLType_Z,
    EPSLType_Q,
    EPSLType_Array,
    EPSLType_Optional,
    EPSLType_Internal
};

union EPSLType_Name {
    enum EPSLBuiltinType_ builtin;
    char *str;
};

struct EPSLBaseType_ {
    bool is_builtin;
    union EPSLType_Name name;
    int32_t bits; // -1 means null
};

struct EPSLType_ {
    struct EPSLBaseType_ base_type_;
    uint32_t generic_count;
    struct EPSLType_ *generics;
};

struct EPSLField {
    struct EPSLType_ *type_;
    char *name;
};

struct EPSLFunc {
    char *symbol;
    struct EPSLType_ *ret_type_; // when void, is NULL
    char *name;
    uint32_t arg_count;
    struct EPSLField *args;
};

struct EPSLStruct {
    char *name;
    uint32_t field_cap;
    uint32_t field_count;
    struct EPSLField **fields;
    bool has_ref_counter;
};

struct CollectedInfo {
    bool is_section_private;
    char *common_prefix;
    char *implementation_location;
    uint32_t struct_cap;
    uint32_t struct_count;
    struct EPSLStruct **structs;
    uint32_t func_cap;
    uint32_t func_count;
    struct EPSLFunc **funcs;
};

struct ArrayStructVisitState {
    struct EPSLType_ *generic;
    uint32_t i;
};

struct ParamCollectorState {
    struct EPSLFunc *func;
    uint32_t i;
};

const char *const builtin_type__names[] = {
    "Bool",
    "Byte",
    "W",
    "Z",
    "Q",
    "Array",
    "Optional",
    "Internal"
};

const enum EPSLBuiltinType_ OptionalableBuiltinTypes_ = EPSLType_Array;

const char *const skip_struct_prefixes[] = {"_", "ARRAY_"};

char *src_filename;

CXTranslationUnit unit;

CXFile file;

#ifndef strdup
char *strdup(const char *src) {
    size_t cap = strlen(src)+1;
    void *result = malloc(cap);
    memcpy(result, src, cap);
    return result;
}
#endif

extern inline uint32_t grow_cap(uint32_t old) {
    return (old * 3) / 2 + 1;
}

bool remove_start(const char *src, const char *start, const char **result) {
    size_t startlen = strlen(start);
    if (startlen > strlen(src)) return false;
    if (memcmp(src, start, startlen) != 0) return false;
    *result = src + startlen;
    return true;
}

bool starts_with_any(const char *str, const char *const prefixes[], uint32_t prefix_count) {
    char prefixes_left[prefix_count];
    memset(prefixes_left, 1, prefix_count);
    uint32_t prefixes_left_count = prefix_count;
    for (uint32_t i = 0; i < strlen(str)+1; i++) {
        char str_char = str[i];
        for (uint32_t j = 0; j < prefix_count; j++) {
            if (prefixes_left[j] == 0) continue;
            char prefix_char = prefixes[j][i];
            if (prefix_char == '\0') return true;
            if (str_char != prefix_char) {
                prefixes_left[j] = 0;
                prefixes_left_count--;
            }
        }
        if (prefixes_left_count == 0) return false;
    }
    return false;
}

void CXStr_puts(CXString str) {
    puts(clang_getCString(str));
    clang_disposeString(str);
}

void output_EPSLBaseType_(struct EPSLBaseType_ *base_type_) {
    if (base_type_->is_builtin) {
        puts(builtin_type__names[base_type_->name.builtin]);
    } else {
        puts(base_type_->name.str);
    }
    printf("%"PRId32"\n", base_type_->bits);
}

void output_EPSLType_(struct EPSLType_ *type_) {
    output_EPSLBaseType_(&type_->base_type_);
    printf("%"PRIu32"\n", type_->generic_count);
    for (uint32_t i = 0; i < type_->generic_count; i++) {
        output_EPSLType_(type_->generics + i);
    }
}

void output_EPSLField(struct EPSLField *field) {
    output_EPSLType_(field->type_);
    puts(field->name);
}

void output_collected(struct CollectedInfo *info) {
    puts("success");

    if (info->implementation_location == NULL) {
        puts("");
    } else {
        puts(info->implementation_location);
    }

    printf("%"PRIu32"\n", info->struct_count);
    for (uint32_t i = 0; i < info->struct_count; i++) {
        puts(info->structs[i]->name);
    }
    for (uint32_t i = 0; i < info->struct_count; i++) {
        struct EPSLStruct *struct_ = info->structs[i];
        puts(struct_->name);
        printf("%"PRIu32"\n", struct_->field_count);
        for (uint32_t j = 0; j < struct_->field_count; j++) {
            output_EPSLField(struct_->fields[j]);
        }
    }

    printf("%"PRIu32"\n", info->func_count);
    for (uint32_t i = 0; i < info->func_count; i++) {
        struct EPSLFunc *func = info->funcs[i];
        puts(func->symbol);
        if (func->ret_type_ == NULL) {
            putchar('\n');
        } else {
            output_EPSLType_(func->ret_type_);
        }
        puts(func->name);
        printf("%"PRIu32"\n", func->arg_count);
        for (uint32_t j = 0; j < func->arg_count; j++) {
            output_EPSLField(func->args + j);
        }
    }
}

unsigned location_to_int(CXSourceLocation location) {
    unsigned line, column, offset;
    clang_getSpellingLocation(location, file, &line, &column, &offset);
    return offset;
}

noreturn void report_error(CXCursor cursor, char *err, ...) {
    puts("input error");

    va_list list;
    va_start(list, err);
    vprintf(err, list);
    va_end(list);

    CXSourceRange range = clang_getCursorExtent(cursor);
    CXSourceLocation start = clang_getRangeStart(range);
    CXSourceLocation end = clang_getRangeEnd(range);
    printf("\n%u\n%u\n", location_to_int(start), location_to_int(end));
    exit(0);
}

noreturn void report_fail(char *err) {
    puts("processing error");
    puts(err);
    exit(0);
}

CXType strip_elaboration(CXType type) {
    if (type.kind == CXType_Elaborated) {
        return clang_Type_getNamedType(type);
    } else {
        return type;
    }
}

bool is_uint_of_size(CXType type, uint32_t bytes) {
    type = strip_elaboration(type);
    switch (type.kind) {
    case CXType_Typedef: {
        CXCursor decl = clang_getTypeDeclaration(type);
        return is_uint_of_size(clang_getTypedefDeclUnderlyingType(decl), bytes);
    }
    case CXType_UChar:
    case CXType_UShort:
    case CXType_UInt:
    case CXType_ULong:
    case CXType_ULongLong:
    case CXType_UInt128:
        return clang_Type_getSizeOf(type) == bytes;
    default:
        return false;
    }
}

void CXType_to_EPSLType_(CXCursor cursor, CXType in, struct EPSLType_ *out);

void CXType_pointee_to_EPSLType_(CXCursor cursor, CXType in, struct EPSLType_ *out);

enum CXChildVisitResult array_struct_visitor(CXCursor field, CXCursor _, CXClientData visit_data) {
    struct ArrayStructVisitState *visit_state = (struct ArrayStructVisitState*)visit_data;

    CXType field_type = clang_getCursorType(field);

    char *expected_name;

    switch (visit_state->i) {
    case 0:
        expected_name = "ref_counter";
        goto is_uint64_t_field;
    case 1:
        expected_name = "capacity";
        goto is_uint64_t_field;
    case 2:
        expected_name = "length";
        goto is_uint64_t_field;
    case 3: {
        expected_name = "content";

        if (field_type.kind != CXType_Pointer) {
            report_error(field, "Field has incorrect type, expected pointer type");
        }
        CXType content_type = clang_getPointeeType(field_type);
        struct EPSLType_ *generic = malloc(sizeof(*generic));
        CXType_to_EPSLType_(field, content_type, generic);
        visit_state->generic = generic;

        goto is_other_field;
    }
    case 4:
        report_error(field, "Too many fields for array struct");
    }

is_uint64_t_field:
    if (!is_uint_of_size(field_type, 8)) {
        report_error(field, "Field has incorrect type, expected type uint64_t");
    }

is_other_field:;
    CXString field_name = clang_getCursorSpelling(field);

    if (strcmp(expected_name, clang_getCString(field_name)) != 0) {
        report_error(field, "Field has incorrect name, expected name %s", expected_name);
    }

    clang_disposeString(field_name);

    visit_state->i++;

    return CXChildVisit_Continue;
}

void array_struct_decl_to_EPSLType_(CXCursor decl, CXType in, struct EPSLType_ *out) {
    struct ArrayStructVisitState visit_state;
    memset(&visit_state, 0, sizeof(visit_state));

    clang_visitChildren(decl, &array_struct_visitor, &visit_state);

    out->base_type_.is_builtin = true;
    out->base_type_.name.builtin = EPSLType_Array;
    out->base_type_.bits = -1;
    out->generic_count = 1;
    out->generics = visit_state.generic;
}

enum CXChildVisitResult ref_counter_check_visitor(CXCursor field, CXCursor _, CXClientData visit_data) {
    bool *ref_counter_start = (bool*)visit_data;

    CXString field_name = clang_getCursorSpelling(field);

    if (strcmp(clang_getCString(field_name), "ref_counter") == 0) {
        CXType field_type = clang_getCursorType(field);
        if (!is_uint_of_size(field_type, 8)) {
            report_error(field, "ref_counter field isn't the right type (expected type uint64_t)");
        }
        *ref_counter_start = true;
    }

    clang_disposeString(field_name);

    return CXChildVisit_Break;
}

bool does_struct_have_ref_counter(CXCursor struct_) {
    bool ref_counter_start = false;
    clang_visitChildren(struct_, &ref_counter_check_visitor, &ref_counter_start);
    return ref_counter_start;
}

void struct_decl_to_EPSLType_(CXCursor decl, const char *name, CXType in, struct EPSLType_ *out) {
    const char *name_base;
    if (remove_start(name, "ARRAY_", &name_base)) {
        array_struct_decl_to_EPSLType_(decl, in, out);
    } else {
        if (does_struct_have_ref_counter(decl)) {
            size_t name_len = strlen(name)+1;
            char *name_copy = malloc(name_len);
            memcpy(name_copy, name, name_len);

            out->base_type_.is_builtin = false;
            out->base_type_.name.str = name_copy;
            out->base_type_.bits = -1;
            out->generic_count = 0;
            out->generics = NULL;
        } else {
            out->base_type_.is_builtin = true;
            out->base_type_.name.builtin = EPSLType_Internal;
            out->base_type_.bits = -1;
            out->generic_count = 0;
            out->generics = NULL;
        }
    }
}

bool check_pointer_typedef_patterns(CXString name, CXString underlying_name, CXCursor typedef_, CXType underlying, struct EPSLType_ *out) {
    const char *name_base;
    if (remove_start(clang_getCString(name), "NULLABLE_", &name_base)) {
        const char *underlying_name_cstr = clang_getCString(underlying_name);
        if (strcmp(underlying_name_cstr, name_base) != 0) {
            report_error(typedef_, "This typedef doesn't reference the correct struct given its name. It's expected it to reference %s (given the typedef name), while it references %s.", name_base, underlying_name_cstr);
        }

        struct EPSLType_ *generic = malloc(sizeof(*generic));
        CXType_pointee_to_EPSLType_(typedef_, underlying, generic);
        if (generic->base_type_.is_builtin
            && (generic->base_type_.name.builtin & OptionalableBuiltinTypes_) == 0) {
            report_error(typedef_, "Can't make an Optional form of this type");
        }

        out->base_type_.is_builtin = true;
        out->base_type_.name.builtin = EPSLType_Optional;
        out->base_type_.bits = -1;
        out->generic_count = 1;
        out->generics = generic;
        return true;
    }
    return false;
}

void pointer_typedef_to_EPSLType_(CXString name, CXCursor typedef_, CXType underlying, struct EPSLType_ *out) {
    underlying = strip_elaboration(underlying);
    if (underlying.kind == CXType_Record) {
        CXCursor underlying_decl = clang_getTypeDeclaration(underlying);
        CXString underlying_name = clang_getCursorSpelling(underlying_decl);
        if (check_pointer_typedef_patterns(name, underlying_name, typedef_, underlying, out))
            return;
        clang_disposeString(underlying_name);
    }
    CXType_pointee_to_EPSLType_(typedef_, underlying, out);
}

void CXType_pointee_to_EPSLType_(CXCursor cursor, CXType in, struct EPSLType_ *out) {
    in = strip_elaboration(in);
    switch (in.kind) {
    case CXType_Void:
        out->base_type_.is_builtin = true;
        out->base_type_.name.builtin = EPSLType_Internal;
        out->base_type_.bits = -1;
        out->generic_count = 0;
        out->generics = NULL;
        break;
    case CXType_Typedef: {
        CXCursor typedef_ = clang_getTypeDeclaration(in);
        CXString name = clang_getTypedefName(in);
        CXType underlying = clang_getTypedefDeclUnderlyingType(typedef_);
        pointer_typedef_to_EPSLType_(name, typedef_, underlying, out);
        clang_disposeString(name);
        break;
    }
    case CXType_Record: {
        CXCursor decl = clang_getTypeDeclaration(in);
        CXString name = clang_getCursorSpelling(decl);
        const char *name_cstr = clang_getCString(name);

        char name_start = *name_cstr;
        if (name_start == '\0') {
            report_error(decl, "Somehow this declaration doesn't have a name");
        } else if (name_start == '_') {
            out->base_type_.is_builtin = true;
            out->base_type_.name.builtin = EPSLType_Internal;
            out->base_type_.bits = -1;
            out->generic_count = 0;
            out->generics = NULL;
        } else if (decl.kind == CXCursor_StructDecl) {
            struct_decl_to_EPSLType_(decl, name_cstr, in, out);
        } else {
            report_error(decl, "This declaration can't be used as an Epsilon type");
        }

        clang_disposeString(name);
        break;
    }
    default:
        report_error(cursor, "Cannot parse C type to Epsilon type");
    }
}

void typedef_to_EPSLType_(CXString name, CXCursor typedef_, CXType underlying, struct EPSLType_ *out) {
    underlying = strip_elaboration(underlying);
    if (underlying.kind == CXType_Pointer) {
        pointer_typedef_to_EPSLType_(name, typedef_, clang_getPointeeType(underlying), out);
    } else {
        CXType_to_EPSLType_(typedef_, underlying, out);
    }
}

void CXType_to_EPSLType_(CXCursor cursor, CXType in, struct EPSLType_ *out) {
    in = strip_elaboration(in);
    switch (in.kind) {
    case CXType_Pointer:
        CXType_pointee_to_EPSLType_(cursor, clang_getPointeeType(in), out);
        break;
    case CXType_Typedef: {
        CXString name = clang_getTypedefName(in);
        CXCursor typedef_ = clang_getTypeDeclaration(in);
        CXType underlying = clang_getTypedefDeclUnderlyingType(typedef_);
        typedef_to_EPSLType_(name, typedef_, underlying, out);
        clang_disposeString(name);
        break;
    }
    case CXType_Bool:
        out->base_type_.is_builtin = true;
        out->base_type_.name.builtin = EPSLType_Bool;
        out->base_type_.bits = -1;
        out->generic_count = 0;
        out->generics = NULL;
        break;
    case CXType_UChar:
        out->base_type_.is_builtin = true;
        out->base_type_.name.builtin = EPSLType_Byte;
        out->base_type_.bits = -1;
        out->generic_count = 0;
        out->generics = NULL;
        break;
    case CXType_Char_S:
    case CXType_Short:
    case CXType_Int:
    case CXType_Long:
    case CXType_LongLong:
    case CXType_Int128:
        out->base_type_.is_builtin = true;
        out->base_type_.name.builtin = EPSLType_Z;
        out->base_type_.bits = clang_Type_getSizeOf(in) * 8;
        out->generic_count = 0;
        out->generics = NULL;
        break;
    case CXType_UShort:
    case CXType_UInt:
    case CXType_ULong:
    case CXType_ULongLong:
    case CXType_UInt128:
        out->base_type_.is_builtin = true;
        out->base_type_.name.builtin = EPSLType_W;
        out->base_type_.bits = clang_Type_getSizeOf(in) * 8;
        out->generic_count = 0;
        out->generics = NULL;
        break;
    case CXType_Half:
    case CXType_Float:
    case CXType_Double:
    case CXType_Float128:
        out->base_type_.is_builtin = true;
        out->base_type_.name.builtin = EPSLType_Q;
        out->base_type_.bits = clang_Type_getSizeOf(in) * 8;
        out->generic_count = 0;
        out->generics = NULL;
        break;
    default:
        report_error(cursor, "Cannot parse C type to Epsilon type");
    }
}

void voidable_CXType_to_EPSLType_(CXCursor cursor, CXType in, struct EPSLType_ **out) {
    if (in.kind == CXType_Void) {
        *out = NULL;
    } else {
        struct EPSLType_ *type_ = malloc(sizeof(*type_));
        CXType_to_EPSLType_(cursor, in, type_);
        *out = type_;
    }
}

enum CXChildVisitResult field_collector_visitor(CXCursor cursor, CXCursor _, CXClientData visit_data) {
    struct EPSLStruct *struct_ = (struct EPSLStruct*)visit_data;

    CXString field_name = clang_getCursorSpelling(cursor);
    const char *name = clang_getCString(field_name);
    if (strcmp(name, "ref_counter") == 0) {
        if (struct_->field_count > 0) {
            report_error(cursor, "The ref_counter must be the struct's first field");
        } else if (struct_->has_ref_counter) {
            report_error(cursor, "A struct can't have more than one ref_counter");
        }
        clang_disposeString(field_name);
        return CXChildVisit_Continue;
    }

    size_t name_len = strlen(name)+1;
    char *name_copy = malloc(name_len);
    memcpy(name_copy, name, name_len);
    clang_disposeString(field_name);

    struct EPSLType_ *type_ = malloc(sizeof(*type_));
    CXType cursor_type = clang_getCursorType(cursor);
    CXType_to_EPSLType_(cursor, cursor_type, type_);

    struct EPSLField *field = malloc(sizeof(*field));
    field->type_ = type_;
    field->name = name_copy;

    uint32_t old_count = struct_->field_count++;
    if (struct_->field_count >= struct_->field_cap) {
        uint32_t new_field_cap = grow_cap(struct_->field_count);
        struct_->fields = realloc(struct_->fields, new_field_cap*sizeof(*struct_->fields));
        struct_->field_cap = new_field_cap;
    }
    struct_->fields[old_count] = field;

    return CXChildVisit_Continue;
}

void collect_struct(struct CollectedInfo *info, CXCursor cursor) {
    if (!clang_isCursorDefinition(cursor)) return;
    if (!does_struct_have_ref_counter(cursor)) return;

    CXString struct_name = clang_getCursorSpelling(cursor);
    const char *name = clang_getCString(struct_name);

    if (*name == '\0') {
        report_error(cursor, "Somehow this struct definition doesn't have a name");
    }

    uint32_t skip_prefix_count = sizeof(skip_struct_prefixes) / sizeof(skip_struct_prefixes[0]);
    if (starts_with_any(name, skip_struct_prefixes, skip_prefix_count)) {
        clang_disposeString(struct_name);
        return;
    }

    char *name_copy = strdup(name);
    clang_disposeString(struct_name);

    struct EPSLStruct *struct_ = malloc(sizeof(*struct_));
    struct_->name = name_copy;
    struct_->field_cap = 0;
    struct_->field_count = 0;
    struct_->fields = NULL;
    struct_->has_ref_counter = false;

    clang_visitChildren(cursor, &field_collector_visitor, struct_);

    uint32_t old_count = info->struct_count++;
    if (info->struct_count >= info->struct_cap) {
        uint32_t new_struct_cap = grow_cap(info->struct_count);
        info->structs = realloc(info->structs, new_struct_cap*sizeof(*info->structs));
        info->struct_cap = new_struct_cap;
    }
    info->structs[old_count] = struct_;
}

enum CXChildVisitResult param_collector_visitor(CXCursor cursor, CXCursor _, CXClientData visit_data) {
    if (cursor.kind != CXCursor_ParmDecl) return CXChildVisit_Continue;

    struct ParamCollectorState *state = (struct ParamCollectorState*)visit_data;
    struct EPSLField *field = state->func->args + state->i++;

    CXString field_name = clang_getCursorSpelling(cursor);
    const char *name = clang_getCString(field_name);
    size_t name_len = strlen(name)+1;
    field->name = malloc(name_len);
    memcpy(field->name, name, name_len);
    clang_disposeString(field_name);

    field->type_ = malloc(sizeof(*field->type_));
    CXType_to_EPSLType_(cursor, clang_getCursorType(cursor), field->type_);

    return CXChildVisit_Continue;
}

void collect_func(struct CollectedInfo *info, CXCursor cursor) {
    CXString func_name = clang_getCursorSpelling(cursor);
    const char *name = clang_getCString(func_name);
    char name_start = *name;

    if (name_start == '\0') {
        report_error(cursor, "Somehow this function prototype doesn't have a name");
    } else if (name_start == '_') {
        clang_disposeString(func_name);
        return;
    }

    char *name_copy = strdup(name);
    if (info->common_prefix != NULL
        && !remove_start(name_copy, info->common_prefix, (const char**)&name_copy)) {
        clang_disposeString(func_name);
        free(name_copy);
        return;
    }
    clang_disposeString(func_name);

    CXString func_symbol = clang_Cursor_getMangling(cursor);
    const char *symbol = clang_getCString(func_symbol);
    size_t symbol_len = strlen(symbol)+1;
    char *symbol_copy = malloc(symbol_len);
    memcpy(symbol_copy, symbol, symbol_len);
    clang_disposeString(func_symbol);

    CXType func_type = clang_getCursorType(cursor);

    struct EPSLFunc *func = malloc(sizeof(*func));
    func->symbol = symbol_copy;
    voidable_CXType_to_EPSLType_(cursor, clang_getResultType(func_type), &func->ret_type_);
    func->name = name_copy;
    func->arg_count = clang_getNumArgTypes(func_type);
    func->args = malloc(sizeof(*func->args) * func->arg_count);

    struct ParamCollectorState param_collector_state = {func, 0};
    clang_visitChildren(cursor, &param_collector_visitor, &param_collector_state);

    uint32_t old_count = info->func_count++;
    if (info->func_count >= info->func_cap) {
        uint32_t new_func_cap = grow_cap(info->func_count);
        info->funcs = realloc(info->funcs, new_func_cap*sizeof(*info->funcs));
        info->func_cap = new_func_cap;
    }
    info->funcs[old_count] = func;
}

void check_is_private(struct CollectedInfo *collected_info, CXCursor cursor) {
    CXString union_name = clang_getCursorSpelling(cursor);
    const char *name = clang_getCString(union_name);
    if (strcmp(name, "___EPSL_PUBLIC_STOP") == 0) {
        collected_info->is_section_private = true;
    } else if (strcmp(name, "___EPSL_PUBLIC_START") == 0) {
        collected_info->is_section_private = false;
    }
    clang_disposeString(union_name);
}

void parse_c_string(CXCursor error_location, char *str) {
    size_t input_len = strlen(str);
    if (input_len < 2 || str[0] != '"' || str[input_len-1] != '"') {
        report_error(error_location, "Expected simple string");
    }

    bool was_backslash = false;
    size_t j = 0;

    for (size_t i = 1; i < input_len-1; i++) {
        char in_char = str[i];
        if (was_backslash) {
            char out_char;
            switch (in_char) {
            case '\'':
                out_char = '\'';
                break;
            case '"':
                out_char = '"';
                break;
            case '?':
                out_char = '?';
                break;
            case '\\':
                out_char = '\\';
                break;
            case 'a':
                out_char = '\a';
                break;
            case 'b':
                out_char = '\b';
                break;
            case 'f':
                out_char = '\f';
                break;
            case 'n':
                out_char = '\n';
                break;
            case 'r':
                out_char = '\r';
                break;
            case 't':
                out_char = '\t';
                break;
            case 'v':
                out_char = '\v';
                break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case 'o':
                report_error(error_location, "Octal escape sequences are currently not supported");
            case 'x':
                report_error(error_location, "Hexadecimal escape sequences are currently not supported");
            case 'u':
            case 'U':
            case 'N':
                report_error(error_location, "Universal character names are not currently supported");
            default:
                report_error(error_location, "Invalid escape sequence");
            }

            str[j++] = out_char;
            was_backslash = false;
        } else {
            if (in_char == '\\') {
                was_backslash = true;
            } else if (in_char == '"') {
                report_error(error_location, "Unescaped \" found in string");
            } else {
                str[j++] = in_char;
            }
        }
    }

    str[j] = '\0';
}

char *get_macro_value(CXCursor macro) {
    CXToken *tokens;
    unsigned int num_tokens;
    clang_tokenize(unit, clang_getCursorExtent(macro), &tokens, &num_tokens);

    if (num_tokens < 2) {
        report_error(macro, "Required value, no value specified");
    } else if (num_tokens > 2) {
        report_error(macro, "Expected one value");
    }

    CXString cx_macro_value = clang_getTokenSpelling(unit, tokens[1]);
    char *macro_value = strdup(clang_getCString(cx_macro_value));
    clang_disposeString(cx_macro_value);

    clang_disposeTokens(unit, tokens, num_tokens);

    return macro_value;
}

void handle_macro(struct CollectedInfo *collected_info, CXCursor cursor) {
    CXString cx_macro_name = clang_getCursorSpelling(cursor);
    const char *macro_name = clang_getCString(cx_macro_name);

    if (strcmp(macro_name, "EPSL_COMMON_PREFIX") == 0) {
        if (collected_info->common_prefix != NULL) {
            free(collected_info->common_prefix);
        }
        char *common_prefix = get_macro_value(cursor);
        parse_c_string(cursor, common_prefix);
        collected_info->common_prefix = common_prefix;
    } else if (strcmp(macro_name, "EPSL_IMPLEMENTATION_LOCATION") == 0) {
        if (collected_info->implementation_location != NULL) {
            free(collected_info->implementation_location);
        }
        char *implementation_location = get_macro_value(cursor);
        parse_c_string(cursor, implementation_location);
        collected_info->implementation_location = implementation_location;
    }

    clang_disposeString(cx_macro_name);
}

enum CXChildVisitResult visit_toplevel(CXCursor cursor, CXCursor _, CXClientData visit_data) {
    struct CollectedInfo *collected_info = (struct CollectedInfo*)visit_data;

    if (!clang_Location_isFromMainFile(clang_getCursorLocation(cursor))) {
        return CXChildVisit_Continue;
    } else if (clang_getCursorLinkage(cursor) == CXLinkage_Internal) {
        return CXChildVisit_Continue;
    } else if (collected_info->is_section_private) {
        if (cursor.kind == CXCursor_UnionDecl) {
            check_is_private(collected_info, cursor);
        }

        return CXChildVisit_Continue;
    } else {
        switch (cursor.kind) {
        case CXCursor_MacroDefinition:
            handle_macro(collected_info, cursor);
        case CXCursor_StructDecl:
            collect_struct(collected_info, cursor);
            return CXChildVisit_Continue;
        case CXCursor_FunctionDecl:
            collect_func(collected_info, cursor);
            return CXChildVisit_Continue;
        case CXCursor_UnionDecl:
            check_is_private(collected_info, cursor);
            return CXChildVisit_Continue;
        default:
            return CXChildVisit_Continue;
        }
    }
}

int main(int argc, const char *const argv[]) {
    if (argc < 2) {
        report_fail("Not enough arguments\n");
    }

    // argv[0] isn't a real argument
    argv++;
    argc--;

    const char *const src_filename = *(argv++);
    argc--;

    CXIndex index = clang_createIndex(0, 0);

    unit = clang_parseTranslationUnit(
        index, src_filename, argv, argc,
        NULL, 0, CXTranslationUnit_DetailedPreprocessingRecord
    );

    if (unit == NULL) {
        report_fail("Failed to parse input C file\n");
    }

    file = clang_getFile(unit, src_filename);

    CXCursor cursor = clang_getTranslationUnitCursor(unit);

    struct CollectedInfo collected_info;
    memset(&collected_info, 0, sizeof(collected_info));

    clang_visitChildren(cursor, &visit_toplevel, &collected_info);

    output_collected(&collected_info);

    return 0;
}
