#!/usr/bin/env python3
from scriptutils import *


def main():
    with open("updates.txt") as file:
        updates_lines = file.readlines()
    # with open("updates.txt", "w") as file:
    #     pass

    updates = {}

    for update_line in updates_lines:
        if not update_line:
            continue
        path, start, stop, *replacement = update_line.split("\t")

        file_updates = updates.get(path, [])
        file_updates.append({
            "start": int(start),
            "stop": int(stop),
            "replacement": replacement
        })
        updates[path] = file_updates

    for path, file_updates in updates.items():
        with chdir(Path(path).parent):
            run_cmd("git", "checkout", "HEAD", "--", path)

        file_updates.sort(key=lambda update: update["start"])

        with open(path) as file:
            og_content = file.read()
        content = og_content

        last_end = 0

        len_delta = 0
        for update in file_updates:
            if update["start"] < last_end:
                print("Skipping overlapping update")
                print(path, ">"+og_content[update["start"]:update["stop"]]+"<")
                continue

            before = content[:update["start"]+len_delta]
            after = content[update["stop"]+1+len_delta:]
            cut_len = update["stop"] - update["start"] + 1

            replacement = list(reversed(update["replacement"]))
            new_txt = ""
            while replacement:
                replacement_type = replacement.pop()
                if replacement_type == "span":
                    start = int(replacement.pop())
                    stop = int(replacement.pop())
                    new_txt += og_content[start:stop+1]
                elif replacement_type == "text":
                    new_txt += replacement.pop()

            content = before + new_txt + after
            len_delta += len(new_txt) - cut_len
            last_end = update["stop"]

        with open(path, "w") as file:
            file.write(content)

if __name__ == "__main__":
    main()
