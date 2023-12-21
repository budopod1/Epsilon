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
            }, -1, new Type_("W", 64)
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "capacity")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()}))
            }, -2, new Type_("W", 64)
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
            }, -3, (List<Type_> types_) => {
                Type_ value = types_[1];
                Type_ generic = types_[0].GetGeneric(0);
                if (!value.IsConvertibleTo(generic))
                    throw new FunctionCallTypes_Exception($"Cannot append type {value} to an array of type {types_[0]}", 1);
                return Type_.Void();
            }
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
            }, -4, Type_.Void()
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "shrink_mem"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, -5, Type_.Void()
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
            }, -6, (List<Type_> types_) => {
                return types_[0].GetGeneric(0);
            }
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
            }, -7, (List<Type_> types_) => {
                Type_ value = types_[2];
                Type_ generic = types_[0].GetGeneric(0);
                if (!value.IsConvertibleTo(generic))
                    throw new FunctionCallTypes_Exception($"Cannot insert type {value} into an array of type {types_[0]}", 2);
                return Type_.Void();
            }
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "clone"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("array", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, -8, (List<Type_> types_) => types_[0]
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
            }, -9, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot extend array of type {types_[0]} with an array of type {types_[1]}", 1);
                return Type_.Void();
            }
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
            }, -10, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot concat array of type {types_[0]} with an array of type {types_[1]}", 1);
                return types_[0];
            }
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "make_range_array"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("end", new Type_("Z", 32)),
            }, -11, new Type_("Array", new List<Type_> {new Type_("Z", 32)})
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
            }, -12, new Type_("Array", new List<Type_> {new Type_("Z", 32)})
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
            }, -13, new Type_("Array", new List<Type_> {new Type_("Z", 32)})
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "abs")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", new Type_("Z"))
            }, -14, new Type_("W")
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "abs")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", new Type_("Q", 64))
            }, -15, new Type_("Q", 64)
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
            }, -16, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot concat array of type {types_[0]} with an array of type {types_[1]}", 1);
                return types_[0];
            }
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "stringify")
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", Type_.Any()),
            }, -17, Type_.String()
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "print"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", Type_.Any()),
            }, -18, Type_.Void()
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new UnitPatternSegment<string>(typeof(Name), "println"),
                    new TypePatternSegment(typeof(RawSquareGroup))
                }, new SlotPatternProcessor(new List<int> {1})
            ), new List<FunctionArgument> {
                new FunctionArgument("value", Type_.Any()),
            }, -19, Type_.Void()
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
            }, -20, Type_.Void()
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
            }, -21, Type_.Void()
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
            }, -22, (List<Type_> types_) => types_[0]
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "countChr"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("str", Type_.String()),
                new FunctionArgument("chr", new Type_("Byte")),
            }, -23, new Type_("W", 64)
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
            }, -24, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot count occurrences of array of type {types_[1]} in an array of type {types_[0]}", 1);
                return new Type_("W", 64);
            }
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "overlapCount"),
                    new TypePatternSegment(typeof(RawSquareGroup)),
                }, new SlotPatternProcessor(new List<int> {0, 3})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
                new FunctionArgument("sub", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, -25, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot count occurrences of array of type {types_[1]} in an array of type {types_[0]}", 1);
                return new Type_("W", 64);
            }
        ), new ExternalFunction(
            new ConfigurablePatternExtractor<List<IToken>>(
                new List<IPatternSegment> {
                    new TypePatternSegment(typeof(RawSquareGroup)),
                    new TextPatternSegment("."),
                    new UnitPatternSegment<string>(typeof(Name), "nest"),
                }, new SlotPatternProcessor(new List<int> {0})
            ), new List<FunctionArgument> {
                new FunctionArgument("arr", new Type_("Array", new List<Type_> {Type_.Any()})),
            }, -26, (List<Type_> types_) => new Type_("Array", types_)
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
            }, -27, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot split array of type {types_[0]} by an array of type {types_[1]}", 1);
                return new Type_("Array", new List<Type_> {types_[0]});
            }
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
            }, -28, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot check if array of type {types_[0]} starts with array of type {types_[1]}", 1);
                return new Type_("Bool");
            }
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
            }, -29, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"Cannot check if array of type {types_[0]} ends with array of type {types_[1]}", 1);
                return new Type_("Bool");
            }
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
            }, -30, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"The 'equals' function can only compare equality of values of equal types_", 1);
                return new Type_("Bool");
            }
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
            }, -31, (List<Type_> types_) => {
                if (!types_[0].Equals(types_[1]))
                    throw new FunctionCallTypes_Exception($"The 'not equals' function can only compare equality of values of equal types_", 1);
                return new Type_("Bool");
            }
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
            }, -32, (List<IValueToken> tokens) => {
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
            }
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
            }, -33, (List<IValueToken> tokens) => {
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
            }
        )
    };
}
