#ifdef _MSC_VER
#define _CRT_SECURE_NO_WARNINGS
#endif

#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#include <string.h>

#include "builtins.h"

struct File {
    uint64_t ref_counter;
    FILE *file;
    uint32_t mode;
    bool open;
};

#define _FILE_READ_MODE 1
#define _FILE_WRITE_MODE 2
#define _FILE_APPEND_MODE 4
#define _FILE_TEXT_MODE 8

// returns File?
struct File *fileio_open_file(struct Array *file_path, uint32_t mode) {
    char mode_str[4];
    uint32_t i = 0;

    if (mode&_FILE_WRITE_MODE) {
        if (mode&_FILE_APPEND_MODE)
            return NULL;
        mode_str[i++] = 'w';
        if (mode&_FILE_READ_MODE)
            mode_str[i++] = '+';
    } else if (mode&_FILE_READ_MODE) {
        mode_str[i++] = 'r';
        if (mode&_FILE_APPEND_MODE)
            mode_str[i++] = '+';
    } else if (mode&_FILE_APPEND_MODE) {
        mode_str[i++] = 'a';
    } else {
        return NULL;
    }

    if (!(mode&_FILE_TEXT_MODE)) {
        mode_str[i++] = 'b';
    }

    mode_str[i] = '\0';

    uint64_t file_path_len = file_path->length;
    char c_file_path[file_path_len+1];
    memcpy(c_file_path, file_path->content, file_path_len);
    c_file_path[file_path_len] = '\0';

    FILE *C_file = fopen(c_file_path, mode_str);
    if (C_file == NULL) return NULL;

    struct File *file = epsl_malloc(sizeof(struct File));
    file->ref_counter = 0;
    file->file = C_file;
    file->mode = mode;
    file->open = true;

    return file;
}

extern inline uint32_t fileio_file_read_mode() {
    return _FILE_READ_MODE;
}

extern inline uint32_t fileio_file_write_mode() {
    return _FILE_WRITE_MODE;
}

extern inline uint32_t fileio_file_append_mode() {
    return _FILE_APPEND_MODE;
}

extern inline uint32_t fileio_file_text_mode() {
    return _FILE_TEXT_MODE;
}

void fileio_close_file(struct File *file) {
    if (!file->open) return;
    fclose(file->file);
    file->open = false;
}

static int64_t _file_binary_len(const struct File *file) {
    if (!file->open) return -1;
    FILE *fp = file->file;
    long start_pos = ftell(fp);
    fseek(fp, 0, SEEK_END);
    uint64_t length = (uint64_t)ftell(fp);
    fseek(fp, start_pos, SEEK_SET);
    return length;
}

int64_t fileio_file_length(const struct File *file) {
    if (file->mode&_FILE_TEXT_MODE) {
        return -1;
    } else {
        return _file_binary_len(file);
    }
}

int64_t fileio_file_pos(const struct File *file) {
    if (!file->open) return -1;
    return (uint64_t)ftell(file->file);
}

// returns: Str?
struct Array *fileio_read_all_file(const struct File *file) {
    uint64_t max_remaining = _file_binary_len(file);
    if (max_remaining == -1) return NULL;

    uint64_t cur_pos = fileio_file_pos(file);
    if (cur_pos == -1) return NULL;
    max_remaining -= cur_pos;

    uint64_t capacity = max_remaining;
    if (capacity == 0) capacity = 1;
    char *content = epsl_malloc(capacity);

    uint64_t length;

    if (file->mode&_FILE_TEXT_MODE) {
        length = 0;
        char *buf_ptr = content;

        int c;
        while ((c = fgetc(file->file)) != EOF) {
            if (length++ >= capacity) {
                free(content);
                return NULL;
            }
            *(buf_ptr++) = c;
        }

        if (!feof(file->file)) {
            free(content);
            return NULL;
        }
    } else {
        length = max_remaining;
        size_t read = fread(content, max_remaining, 1, file->file);
        if (read != 1) {
            free(content);
            return NULL;
        }
    }

    struct Array *result = epsl_malloc(sizeof(*result));
    result->ref_counter = 0;
    result->capacity = capacity;
    result->length = length;
    result->content = content;
    return result;
}

// returns: Str?
struct Array *fileio_read_some_file(const struct File *file, uint64_t amount) {
    if (!file->open) return NULL;
    uint64_t capacity = amount;
    if (capacity == 0) capacity = 1;
    struct Array *result = epsl_malloc(sizeof(struct Array));
    result->ref_counter = 0;
    result->capacity = capacity;
    result->length = amount;
    char *content = epsl_malloc(capacity);
    result->content = content;
    size_t read = fread(content, amount, 1, file->file);
    if (read != 1) {
        free(result);
        free(content);
        return NULL;
    }
    return result;
}

bool fileio_set_file_pos(const struct File *file, uint64_t pos) {
    if (!file->open) return false;
    return fseek(file->file, (long)pos, SEEK_SET) == 0;
}

bool fileio_jump_file_pos(const struct File *file, uint64_t amount) {
    if (!file->open) return false;
    return fseek(file->file, (long)amount, SEEK_CUR) == 0;
}

static int64_t _fileio_read_line(char **line, size_t *cap, FILE *file) {
    if (*line == NULL) {
        *cap = 0;
    }
    size_t len = 0;

    while (true) {
        int c = fgetc(file);
        if (ferror(file)) return -1;
        if (c == EOF || c == '\n') return len;
        if (len == *cap) {
            *cap = (*cap * 3) / 2 + 64;
            *line = epsl_realloc(*line, *cap);
        }
        (*line)[len++] = c;
    }
}

static bool read_line_EOF = false;

// returns: Str?
struct Array *fileio_read_file_line(const struct File *file) {
    read_line_EOF = false;
    if (!file->open) return NULL;
    char *content = NULL;
    size_t capacity = 0;
    int64_t len = (int64_t)_fileio_read_line(&content, &capacity, file->file);
    if (len == -1) {
        free(content);
        read_line_EOF = true;
        return NULL;
    }
    if (content[len-1] == '\n') len--;
    struct Array *result = epsl_malloc(sizeof(struct Array));
    result->ref_counter = 0;
    result->capacity = capacity;
    result->content = content;
    result->length = len;
    return result;
}

bool fileio_read_line_reached_EOF() {
    return read_line_EOF;
}

// returns [Str]?
struct Array *fileio_read_file_lines(const struct File *file) {
    struct Array *result = epsl_blank_array(sizeof(struct Array));
    while (1) {
        struct Array *line = fileio_read_file_line(file);
        if (read_line_EOF) {
            return result;
        }
        if (line == NULL) {
            for (uint64_t i = 0; i < result->length; i++) {
                struct Array *str = ((struct Array**)result->content)[i];
                free(str->content);
                free(str);
            }
            free(result->content);
            free(result);
            return NULL;
        }
        uint64_t length = result->length;
        epsl_increment_length(result, sizeof(struct Array));
        ((struct Array**)result->content)[length] = line;
        line->ref_counter = 1;
    }
}

bool fileio_write_to_file(const struct File *file, const struct Array *text) {
    if (!file->open) return false;
    uint64_t len = text->length;
    return fwrite(text->content, len, 1, file->file) == 1;
}

void fileio_free_file(struct File *file) {
    // We don't need a check for if the file is already closed, becauuse
    // close_file contains one itself
    fileio_close_file(file);
    free(file);
}
