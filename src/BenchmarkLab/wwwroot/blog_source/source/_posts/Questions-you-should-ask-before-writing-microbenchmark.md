---
title: Questions you should ask before optimizing performance and writing microbenchmark
date: 2018-06-06 10:06:51
tags:
- microbenchmarking
- howto
---
Multiple [studies](https://www.webperformance.com/load-testing-tools/blog/2010/06/microsoft-affirms-the-importance-of-web-performance/) have shown that performance is the business-critical requirement for the software. For example, [this study](https://www.fastcompany.com/1825005/how-one-second-could-cost-amazon-16-billion-sales) from Amazon shows that 1 second performance degradation can cost $1.6B in sales revenue (yes, billions). It means that even though performance is [non-functional software requirement](https://en.wikipedia.org/wiki/Non-functional_requirement), it has difect effect on the key business metrics and it is highly critical to treat it seriously. 

![charts](https://user-images.githubusercontent.com/3173477/41115761-514a9b32-6a3d-11e8-9269-b506a99f03b9.png)


Software engineers have multiple tools & approaches that help to tackle the problem with performance. One such tool is `benchmarking`. [Wikipedia](https://en.wikipedia.org/wiki/Benchmark_(computing)) defines benchmarking as "the act of running a computer program, a set of programs, or other operations, in order to assess the relative performance of an object, normally by running a number of standard tests and trials against it". 

`Microbenchmarking` is the type of benchmarking which is focused on specific and isolated subset of the software. Microbenchmarking is the powerful tool which enables developer not only to reason about performance, but also make an attempts to improve it.

However, as every tool, it can be misused which, in best case, will not provide any effect, in the worst case it will lead to wasted time, increased maintenance costs and the opposite effect (performance may degrade).

# Are you optimizing performance at the appropriate time?
Donald Knuth once wrote: _"Programmers waste enormous amounts of time thinking about, or worrying about, the speed of noncritical parts of their programs, and these attempts at efficiency actually have a strong negative impact when debugging and maintenance are considered. We should forget about small efficiencies, say about 97% of the time: premature optimization is the root of all evil. Yet we should not pass up our opportunities in that critical 3%."_

This paragraph was first published in 1974 and it still remains true. While performance is truly important, it is also important to know when to spend time on the optimization. If you do it too early - you are shooting in the dark. Do you know what is the typical workload  of the system? Or you just guessing? Does it even need to be optimized? Maybe its performance is already good enough and the bottleneck is in the other feature. 
Another thing: what is more important at the current stage? Is it a new feature? It is particularly important for the growing companies/products: competitors  may have somewhat slower system but with more features and eventually kick you out of business. 

*So do the first things first.*

# Do you know your baseline?
How do you know that something had been improved? To answer question we need to know the state of the things before and after our change. Without knowing original state of the things we can't really know where are we going.

To measure the impact of the change we need to know the original state, the baseline. Without it we can't really claim that something was improved.

Picking good metrics as baseline is crucial. Baseline must represent things that are important for the users: response time at p99, responsiveness of the UI operations, how quickly app becomes interactive, how fast something can be delivered and rendered in the browser, etc. It is also important to measure things from client side, not just server side.

Also, application workloads are typically not uniform - some features used more often, by more people. And sometimes you might even care more about minority of the users - if those users generate more revenue for the business. 

Microbenchmark might give false impression that performance was improved by X%, but in reality that improvement affected only tiny % of users or scenarios, and degraded performance in the other parts of the system. One can't possibly know that without proper measurements and established baseline.

*Know your baseline*

# Is microbenchmarking even the right tool?

 Ok, so you know your workload , and it is pretty important to optimize some particular piece of the hot path in the system. You have few ideas how to rewrite the code to perform faster. But is microbenchmark is the best solution here? 

 The problem with microbenchmarks is that it is too isolated and calls for ideal conditions. You can only test very small parts of the application as if the rest of the world does not exist. Sometimes it will work, sometimes we need to look at the bigger picture. What if the change needs to be more global and affect large chunk of the system? Maybe, instead of trying to optimize not efficient algorithm or architecture , it is time to make a radical change? Or just try to avoid doing unnecessary work instead of trying to improve the speed.

 For example, your app might perform some work whenever user scrolls the page in the browser. Lets say it repositions some DOM element on the page using JavaScript and  styles:

 ```
 window.onscroll = doThisStuffOnScroll;

function doThisStuffOnScroll() {
    // DOM manipulations here
}
 ```

 This is a sure way to drop FPS and make scrolling to feel sluggish. The problem with this code is that `doThisStuffOnScroll` will be called on every pixel scrolled.
First hunch to fix this is to make `doThisStuffOnScroll` work faster: maybe it uses jQuery and you know that vanilla js (document.getElementById, etc) is faster. You write your microbenchmark, see 30% improvement and make a change. *But it is not the best way to handle this.* Instead, in this case, we need to avoid doing unnecessary work. Browsers can re-render the page at every fixed interval, and changes you make in between won't be visible to user. So the best way here would be to schedule the work on scroll event instead of performing the work. (More information about scroll performance can be found here: [How to develop high performance onScroll event?
](http://joji.me/en-us/blog/how-to-develop-high-performance-onscroll-event))

One more awesome article from Paul Lewis: [Scrolling Performance](https://www.html5rocks.com/en/tutorials/speed/scrolling/)

 *Always consider alternatives*


# Do you know what you are really testing?

As mentioned in the previous chapter, microbenchmarks are ran under idealized conditions. Many aspects of the code can be different from the real application, like: 
* state of caches
* JIT Optimizations
* Compiler optimizations
* memory management and garbage collection

There can be many factors that skew the result of microbenchmarks. Compiler optimizations is one of them. If the operation being measured takes so little time that whatever you use to measure it takes longer than the actual operation itself, your microbenchmarks will be skewed also.

For example, someone might take a microbenchmark of the overhead of for loops:
```
void TestForLoop()
{
    time start = GetTime();

    for(int i = 0; i < 1000000000; ++i)
    {
    }

    time elapsed = GetTime() - start;
    time elapsedPerIteration = elapsed / 1000000000;
    printf("Time elapsed for each iteration: %d\n", elapsedPerIteration);
}
```

Obviously compilers can see that the loop does absolutely nothing and not generate any code for the loop at all. So the value of elapsed and elapsedPerIteration is pretty much useless.

Even if the loop does something:
```
void TestForLoop()
{
    int sum = 0;
    time start = GetTime();

    for(int i = 0; i < 1000000000; ++i)
    {
        ++sum;
    }

    time elapsed = GetTime() - start;
    time elapsedPerIteration = elapsed / 1000000000;
    printf("Time elapsed for each iteration: %d\n", elapsedPerIteration);
}
```
The compiler may see that the variable sum isn't going to be used for anything and optimize it away, and optimize away the for loop as well. But wait! What if we do this:
```
void TestForLoop()
{
    int sum = 0;
    time start = GetTime();

    for(int i = 0; i < 1000000000; ++i)
    {
        ++sum;
    }

    time elapsed = GetTime() - start;
    time elapsedPerIteration = elapsed / 1000000000;
    printf("Time elapsed for each iteration: %d\n", elapsedPerIteration);
    printf("Sum: %d\n", sum); // Added
}
```
The compiler might be smart enough to realize that sum will always be a constant value, and optimize all that away as well. Many would be surprised at the optimizing capabilities of compilers these days.

But what about things that compilers can't optimize away?

```
void TestFileOpenPerformance()
{
    FILE* file = NULL;
    time start = GetTime();

    for(int i = 0; i < 1000000000; ++i)
    {
        file = fopen("testfile.dat");
        fclose(file);
    }

    time elapsed = GetTime() - start;
    time elapsedPerIteration = elapsed / 1000000000;
    printf("Time elapsed for each file open: %d\n", elapsedPerIteration);
}
```
Even this is not a useful test! The operating system may see that the file is being opened very frequently, so it may preload it in memory to improve performance. Pretty much all operating systems do this. The same thing happens when you open applications - operating systems may figure out the top ~5 applications you open the most and preload the application code in memory when you boot up the computer!

In fact, there are countless variables that come into play: locality of reference (e.g. arrays vs. linked lists), effects of caches and memory bandwidth, compiler inlining, compiler implementation, compiler switches, number of processor cores, optimizations at the processor level, operating system schedulers, operating system background processes, etc.

So microbenchmarking isn't exactly a useful metric in a lot of cases. It definitely does not replace whole-program benchmarks with well-defined test cases (profiling). Write readable code first, then profile to see what needs to be done, if any.

(from: https://stackoverflow.com/a/2842707/250849)

# Summary

So microbenchmarking is the tool that, if properly used, can help make performance of the software better, which it turn will lead to customer sutatisfaction, better experience, smooth UI and, ultimately, more revenue. But, as any other tool, it can be misused and it is important to know when and how to use it. This post outlines few questions one need to ask himself before writing microbenchmark and using its results.