using System;
using System.Linq;
using System.Collections.Generic;

public class ConfigurableAdvancedPatternExtractor<T> : AdvancedPatternExtractor<T> {
    public ConfigurableAdvancedPatternExtractor(
        List<IPatternSegment> start, List<IPatternSegment> repeated, int minRepeats,
        int maxRepeats, List<IPatternSegment> end, IPatternProcessor<T> processor) {
        this.start = start;
        this.repeated = repeated;
        this.minRepeats = minRepeats;
        this.maxRepeats = maxRepeats;
        this.end = end;
        this.processor = processor;
    }
}
