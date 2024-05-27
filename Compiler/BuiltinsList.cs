using System;
using System.Collections.Generic;

public static class BuiltinsList {
    public static List<ExternalFunction> Builtins = new List<ExternalFunction> {
        new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "length")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()}))
            }, "builtin1", new Type_("W", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "capacity")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()}))
            }, "builtin2", new Type_("W", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "append"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("value", Type_.Any())
            }, "builtin3", (List<Type_> types_) => {
                Type_ value = types_[1];
                Type_ generic = types_[0].GetGeneric(0);
                if (!value.IsConvertibleTo(generic))
                    throw new FunctionCallTypes_Exception($"Cannot append type {value} to an array of type {types_[0]}", 1);
                return Type_.Void();
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "require_capacity"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("value", new Type_("W", 64))
            }, "buitlins4", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "shrink_mem"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin5", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "pop"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("index", new Type_("W", 64)),
            }, "buitlins6", (List<Type_> types_) => {
                return types_[0].GetGeneric(0);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "insert"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3, 4})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("index", new Type_("W", 64)),
                new FunctionArgument("value", Type_.Any()),
            }, "builtin7", (List<Type_> types_) => {
                Type_ value = types_[2];
                Type_ generic = types_[0].GetGeneric(0);
                if (!value.IsConvertibleTo(generic))
                    throw new FunctionCallTypes_Exception($"Cannot insert type {value} into an array of type {types_[0]}", 2);
                return Type_.Void();
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "clone"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin8", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "extend"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("array2", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin9", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot extend array of type {types_[0]} with an array of type {types_[1]}", 1);
                return Type_.Void();
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "concat"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("array1", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("array2", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin10", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot concat array of type {types_[0]} with an array of type {types_[1]}", 1);
                return types_[0];
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "make_range_array"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("end", new Type_("Z", 32)),
            }, "builtin11", new Type_("Array", new List<Type_> {new Type_("Z", 32)}), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "make_range_array"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1, 2})
            ), new List<FunctionArgument> {
                new FunctionArgument("start", new Type_("Z", 32)),
                new FunctionArgument("end", new Type_("Z", 32)),
            }, "builtin12", new Type_("Array", new List<Type_> {new Type_("Z", 32)}), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "make_range_array"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1, 2, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("start", new Type_("Z", 32)),
                new FunctionArgument("end", new Type_("Z", 32)),
                new FunctionArgument("step", new Type_("Z", 32))
            }, "builtin13", new Type_("Array", new List<Type_> {new Type_("Z", 32)}), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "abs")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", new Type_("Z"))
            }, "builtin14", new Type_("W"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "abs")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", new Type_("Q", 64))
            }, "builtin15", new Type_("Q", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("+"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 2})
            ), new List<FunctionArgument> {
                new FunctionArgument("array1", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("array2", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin16", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot concat array of type {types_[0]} with an array of type {types_[1]}", 1);
                return types_[0];
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "stringify")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", Type_.Any()),
            }, "builtin17", Type_.String(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "print"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", Type_.Any()),
            }, "builtin18", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "println"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", Type_.Any()),
            }, "builtin19", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "left_pad"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3, 4})
            ), new List<FunctionArgument> {
                new FunctionArgument("str", Type_.String()),
                new FunctionArgument("len", new Type_("W", 64)),
                new FunctionArgument("chr", new Type_("Byte")),
            }, "builtin20", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "right_pad"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3, 4})
            ), new List<FunctionArgument> {
                new FunctionArgument("str", Type_.String()),
                new FunctionArgument("len", new Type_("W", 64)),
                new FunctionArgument("chr", new Type_("Byte")),
            }, "builtin21", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "slice"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3, 4})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("start", new Type_("W", 64)),
                new FunctionArgument("end", new Type_("W", 64)),
            }, "builtin22", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "count_chr"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("str", Type_.String()),
                new FunctionArgument("chr", new Type_("Byte")),
            }, "builtin23", new Type_("W", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "count"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("sub", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin24", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot count occurrences of array of type {types_[1]} in an array of type {types_[0]}", 1);
                return new Type_("W", 64);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "overlap_count"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("sub", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin25", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot count occurrences of array of type {types_[1]} in an array of type {types_[0]}", 1);
                return new Type_("W", 64);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "nest"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin26", (List<Type_> types_) => new Type_("Array", types_), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "split"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("sub", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin27", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot split array of type {types_[0]} by an array of type {types_[1]}", 1);
                return new Type_("Array", new List<Type_> {types_[0]});
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "starts_with"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("sub", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin28", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot check if array of type {types_[0]} starts with array of type {types_[1]}", 1);
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "ends_with"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("sub", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin29", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot check if array of type {types_[0]} ends with array of type {types_[1]}", 1);
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new UnitPatternSegment<string>(typeof(Name), "equals"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 2})
            ), new List<FunctionArgument> {
                new FunctionArgument("v1", Type_.Any()),
                new FunctionArgument("v2", Type_.Any()),
            }, "builtin30", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"The 'equals' function can only compare equality of values of equal types_", 1);
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new UnitPatternSegment<string>(typeof(Name), "not"),
                    new UnitPatternSegment<string>(typeof(Name), "equals"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("v1", Type_.Any()),
                new FunctionArgument("v2", Type_.Any()),
            }, "builtin31", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"The 'not equals' function can only compare equality of values of equal types_", 1);
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new UnitPatternSegment<string>(typeof(Name), "equals"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new UnitPatternSegment<string>(typeof(Name), "depth"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 2, 4})
            ), new List<FunctionArgument> {
                new FunctionArgument("v1", Type_.Any()),
                new FunctionArgument("v2", Type_.Any()),
                new FunctionArgument("depth", new Type_("W"))
            }, "builtin32", (List<IValueToken> tokens) => {
                Type_ type_1 = tokens[0].GetType_();
                Type_ type_2 = tokens[1].GetType_();
                if (!type_1.Equals(type_2))
                    throw new SyntaxErrorException($"The 'equals' function can only compare equality of values of equal types_", tokens[1]);
                ConstantValue cv = tokens[2] as ConstantValue;
                if (cv == null) {
                    throw new SyntaxErrorException($"The 'equals' function's depth must be a constant", tokens[2]);
                }
                UnsignedIntConstant uic = cv.GetValue() as UnsignedIntConstant;
                if (uic == null) {
                    throw new SyntaxErrorException($"The 'equals' function's depth must be an unsigned integer", tokens[2]);
                }
                int depth = uic.GetIntValue();
                Type_ type_ = type_1;
                while (depth > 0) {
                    if (depth > 1) {
                        if (!(type_.GetBaseType_().GetName() == "Array")) {
                            throw new SyntaxErrorException($"The depth given for the 'equals' function is too high for given type", tokens[0]);
                        }
                        type_ = type_.GetGeneric(0);
                    }
                    depth--;
                }
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new UnitPatternSegment<string>(typeof(Name), "not"),
                    new UnitPatternSegment<string>(typeof(Name), "equals"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new UnitPatternSegment<string>(typeof(Name), "depth"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3, 5})
            ), new List<FunctionArgument> {
                new FunctionArgument("v1", Type_.Any()),
                new FunctionArgument("v2", Type_.Any()),
                new FunctionArgument("depth", new Type_("W"))
            }, "builtin33", (List<IValueToken> tokens) => {
                Type_ type_1 = tokens[0].GetType_();
                Type_ type_2 = tokens[1].GetType_();
                if (!type_1.Equals(type_2))
                    throw new SyntaxErrorException($"The 'not equals' function can only compare equality of values of equal types_", tokens[1]);
                ConstantValue cv = tokens[2] as ConstantValue;
                if (cv == null) {
                    throw new SyntaxErrorException($"The 'not equals' function's depth must be a constant", tokens[2]);
                }
                UnsignedIntConstant uic = cv.GetValue() as UnsignedIntConstant;
                if (uic == null) {
                    throw new SyntaxErrorException($"The 'not equals' function's depth must be an unsigned integer", tokens[2]);
                }
                int depth = uic.GetIntValue();
                Type_ type_ = type_1;
                while (depth > 0) {
                    if (depth > 1) {
                        if (!(type_.GetBaseType_().GetName() == "Array")) {
                            throw new SyntaxErrorException($"The depth given for the 'not equals' function is too high for given type", tokens[0]);
                        }
                        type_ = type_.GetGeneric(0);
                    }
                    depth--;
                }
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "join"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {
                    new Type_("Array", new List<Type_> {Type_.Any()})
                })),
                new FunctionArgument("sep", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin34", (List<Type_> types_) => {
                if (!(types_[0].GetGeneric(0)).Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot join of array of type {types_[0]} on array of type {types_[1]}", 1);
                return types_[1];
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "index_of"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("elem", Type_.Any()),
            }, "builtin35", (List<Type_> types_) => {
                if (!types_[1].IsConvertibleTo(types_[0].GetGeneric(0)))
                    throw new FunctionCallTypes_Exception($"Cannot check index of element of type {types_[1]} in array of type {types_[0]}", 1);
                return new Type_("Z", 64);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "index_of_subsection"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("sub", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, "builtin36", (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot check index of array of type {types_[1]} in array of type {types_[0]}", 1);
                return new Type_("Z", 64);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "parse_int"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("str", Type_.String()),
            }, "builtin37", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "invalid_parsed_int"),
                }, new EmptyPatternProcessor()
            ), new List<FunctionArgument> {}, "builtin38", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "parse_float"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("str", Type_.String()),
            }, "builtin39", new Type_("Q", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "is_NaN"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("val", new Type_("Q", 64)),
            }, "builtin40", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "read_input_line"),
                }, new EmptyPatternProcessor()
            ), new List<FunctionArgument>(), "builtin41", Type_.String(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "open_file"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1, 2})
            ), new List<FunctionArgument> {
                new FunctionArgument("str", Type_.String()),
                new FunctionArgument("mode", new Type_("Z", 32)),
            }, "builtin42", new Type_("File"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "FILE_READ_MODE"),
                }, new EmptyPatternProcessor()
            ), new List<FunctionArgument> {}, "builtin43", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "FILE_WRITE_MODE"),
                }, new EmptyPatternProcessor()
            ), new List<FunctionArgument> {}, "builtin44", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "FILE_APPEND_MODE"),
                }, new EmptyPatternProcessor()
            ), new List<FunctionArgument> {}, "builtin45", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "FILE_BINARY_MODE"),
                }, new EmptyPatternProcessor()
            ), new List<FunctionArgument> {}, "builtin46", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "is_open"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
            }, "builtin47", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "mode"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
            }, "builtin48", new Type_("Z", 32), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "close"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
            }, "builtin49", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "length"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
            }, "builtin50", new Type_("Z", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "pos"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
            }, "builtin51", new Type_("Z", 64), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "read_all"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
            }, "builtin52", new Type_("Optional", new List<Type_> {Type_.String()}), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "read_some"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
                new FunctionArgument("bytes", new Type_("W", 64))
            }, "builtin53", new Type_("Optional", new List<Type_> {Type_.String()}), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "set_pos"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
                new FunctionArgument("pos", new Type_("W", 64))
            }, "builtin54", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "jump_pos"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
                new FunctionArgument("amount", new Type_("W", 64))
            }, "builtin55", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "read_line"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File"))
            }, "builtin56", new Type_("Optional", new List<Type_> {Type_.String()}), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "read_line_reached_EOF"),
                }, new EmptyPatternProcessor()
            ), new List<FunctionArgument> {}, "builtin57", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "read_lines"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File"))
            }, "builtin58", new Type_("Optional", new List<Type_> {
                new Type_("Array", new List<Type_> {Type_.String()})
            }), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "write"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("file", new Type_("File")),
                new FunctionArgument("content", Type_.String())
            }, "builtin59", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "is_null")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("nullable", Type_.Any()),
            }, "builtin60", (List<Type_> types_) => {
                if (!types_[0].GetBaseType_().IsNullable()) {
                    throw new FunctionCallTypes_Exception(
                        $"Type {types_[0]} is never null", 0
                    );
                }
                return new Type_("Bool");
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "unwrap")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("optional", new Type_(
                    "Optional", new List<Type_> {Type_.Any()}
                )),
            }, "builtin61", (List<Type_> types_) => {
                if (types_[0].GetBaseType_().GetName() != "Optional") {
                    throw new FunctionCallTypes_Exception(
                        $"Cannot unwrap non Optional type {types_[0]}", 0
                    );
                }
                return types_[0].GetGeneric(0);
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "blank_from_type"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("amount", new Type_("W", 64))
            }, "builtin62", (List<Type_> types_) => {
                BaseType_ bt = types_[0].GetGeneric(0).GetBaseType_();
                if (!bt.IsNumber() && !bt.IsNullable()) {
                    throw new FunctionCallTypes_Exception(
                        $"Cannot make blank array of type {types_[0]}", 0
                    );
                }
                return types_[0];
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "unique")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()}))
            }, "builtin63", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "sort")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()}))
            }, "builtin64", (List<Type_> types_) => {
                if (!types_[0].GetGeneric(0).GetBaseType_().IsNumber()) {
                    throw new FunctionCallTypes_Exception(
                        $"Only arrays of numbers can be sorted, not {types_[0]}", 0
                    );
                }
                return Type_.Void();
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "sort_inverted")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()}))
            }, "builtin65", (List<Type_> types_) => {
                if (!types_[0].GetGeneric(0).GetBaseType_().IsNumber()) {
                    throw new FunctionCallTypes_Exception(
                        $"Only arrays of numbers can be sorted, not {types_[0]}", 0
                    );
                }
                return Type_.Void();
            }, FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "dedup")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()}))
            }, "builtin66", Type_.Void(), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "repeat"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("times", new Type_("W", 64))
            }, "builtin67", (List<Type_> types_) => types_[0], FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "truthy")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", Type_.Any())
            }, "builtin68", new Type_("Bool"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "floor")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", new Type_("Q"))
            }, "builtin69", new Type_("Z"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "ceil")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", new Type_("Q"))
            }, "builtin70", new Type_("Z"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "round")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", new Type_("Q"))
            }, "builtin71", new Type_("Z"), FunctionSource.Builtin
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "inner")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("optional", new Type_(
                    "Optional", new List<Type_> {Type_.Any()}
                )),
            }, "builtin72", (List<Type_> types_) => {
                if (types_[0].GetBaseType_().GetName() != "Optional") {
                    throw new FunctionCallTypes_Exception(
                        $"Cannot get inner of non Optional type {types_[0]}", 0
                    );
                }
                return types_[0].GetGeneric(0);
            }, FunctionSource.Builtin
        )
    };
}
