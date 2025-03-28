declare class Benchmark {
    static filter<T>(arr: T[], callback: (value: T) => any, thisArg?: any): T[];
    static filter<T>(arr: T[], filter: string, thisArg?: any): T[];
    static formatNumber(num: number): string;
    static join(obj: Object, separator1?: string, separator2?: string): string;
    static invoke(benches: Benchmark[], name: string | Object, ...args: any[]): any[];
    static runInContext(context: Object): Function;

    static each(obj: Object | any[], callback: Function, thisArg?: any): void;
    static forEach<T>(arr: T[], callback: (value: T) => any, thisArg?: any): void;
    static forOwn(obj: Object, callback: Function, thisArg?: any): void;
    static has(obj: Object, path: any[] | string): boolean;
    static indexOf<T>(arr: T[], value: T, fromIndex?: number): number;
    static map<T, K>(arr: T[], callback: (value: T) => K, thisArg?: any): K[];
    static reduce<T, K>(arr: T[], callback: (accumulator: K, value: T) => K, thisArg?: any): K;

    static options: Benchmark.Options;
    static platform: Benchmark.Platform;
    static support: Benchmark.Support;
    static version: string;

    constructor(fn: Function | string, options?: Benchmark.Options);
    constructor(name: string, fn: Function | string, options?: Benchmark.Options);
    constructor(name: string, options?: Benchmark.Options);
    constructor(options: Benchmark.Options);

    id: number;
    name?: string;
    count: number;
    cycles: number;
    hz: number;
    compiled: Function | string;
    error: Error;
    fn: Function | string;
    aborted: boolean;
    running: boolean;
    setup: Function | string;
    teardown: Function | string;

    stats: Benchmark.Stats;
    times: Benchmark.Times;

    abort(): Benchmark;
    clone(options: Benchmark.Options): Benchmark;
    compare(benchmark: Benchmark): number;
    emit(type: string | Object): any;
    listeners(type: string): Function[];
    off(type?: string, listener?: Function): Benchmark;
    off(types: string[]): Benchmark;
    on(type?: string, listener?: Function): Benchmark;
    on(types: string[]): Benchmark;
    reset(): Benchmark;
    run(options?: Benchmark.Options): Benchmark;
    toString(): string;
}

declare namespace Benchmark {
    export interface Options {
        async?: boolean | undefined;
        defer?: boolean | undefined;
        delay?: number | undefined;
        id?: string | undefined;
        initCount?: number | undefined;
        maxTime?: number | undefined;
        minSamples?: number | undefined;
        minTime?: number | undefined;
        name?: string | undefined;
        onAbort?: Function | undefined;
        onComplete?: Function | undefined;
        onCycle?: Function | undefined;
        onError?: Function | undefined;
        onReset?: Function | undefined;
        onStart?: Function | undefined;
        setup?: Function | string | undefined;
        teardown?: Function | string | undefined;
        fn?: Function | string | undefined;
        queued?: boolean | undefined;
    }

    export interface Platform {
        description: string;
        layout: string;
        product: string;
        name: string;
        manufacturer: string;
        os: string;
        prerelease: string;
        version: string;
        toString(): string;
    }

    export interface Support {
        browser: boolean;
        timeout: boolean;
        decompilation: boolean;
    }

    export interface Stats {
        moe: number;
        rme: number;
        sem: number;
        deviation: number;
        mean: number;
        sample: any[];
        variance: number;
    }

    export interface Times {
        cycle: number;
        elapsed: number;
        period: number;
        timeStamp: number;
    }

    export class Deferred {
        constructor(clone: Benchmark);

        benchmark: Benchmark;
        cycles: number;
        elapsed: number;
        timeStamp: number;

        resolve(): void;
    }

    export interface Target {
        options: Options;
        async?: boolean | undefined;
        defer?: boolean | undefined;
        delay?: number | undefined;
        initCount?: number | undefined;
        maxTime?: number | undefined;
        minSamples?: number | undefined;
        minTime?: number | undefined;
        name?: string | undefined;
        fn?: Function | undefined;
        id: number;
        stats?: Stats | undefined;
        times?: Times | undefined;
        running: boolean;
        count?: number | undefined;
        compiled?: Function | undefined;
        cycles?: number | undefined;
        hz?: number | undefined;
    }

    export class Event {
        constructor(type: string | Object);

        aborted: boolean;
        cancelled: boolean;
        currentTarget: Object;
        result: any;
        target: Target;
        timeStamp: number;
        type: string;
    }

    export class Suite {
        static options: { name: string };

        constructor(name?: string, options?: Options);
        constructor(options?: Options);

        length: number;
        aborted: boolean;
        running: boolean;
        name?: string;

        abort(): Suite;
        add(name: string, fn: Function | string, options?: Options): Suite;
        add(fn: Function | string, options?: Options): Suite;
        add(name: string, options?: Options): Suite;
        add(options: Options): Suite;
        clone(options: Options): Suite;
        emit(type: string | Object): any;
        filter(callback: Function | string): Suite;
        join(separator?: string): string;
        listeners(type: string): Function[];
        off(type?: string, callback?: Function): Suite;
        off(types: string[]): Suite;
        on(type?: string, callback?: Function): Suite;
        on(types: string[]): Suite;
        push(benchmark: Benchmark): number;
        reset(): Suite;
        run(options?: Options): Suite;
        reverse(): any[];
        sort(compareFn: (a: any, b: any) => number): any[];
        splice(start: number, deleteCount?: number): any[];
        unshift(benchmark: Benchmark): number;

        each(callback: Function): Suite;
        forEach(callback: Function): Suite;
        indexOf(value: any): number;
        map(callback: Function | string): any[];
        reduce<T>(callback: Function, accumulator: T): T;

        pop(): Function;
        shift(): Benchmark;
        slice(start: number, end: number): any[];
        slice(start: number, deleteCount: number, ...values: any[]): any[];
    }
}

export = Benchmark;