import traceback


open("pylog.txt", "w").close()


try:
    import _create_ir
    _create_ir.main()
except Exception as e:
    with open("pylog.txt", "w") as log:
        log.write(traceback.format_exc())
