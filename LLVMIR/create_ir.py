import traceback


try:
    import _create_ir
    _create_ir.main()
except Exception as e:
    with open("pyerr.txt", "w") as log:
        log.write(traceback.format_exc())
