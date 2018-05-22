---
title: What is new in version 1.2.53.0
date: 2018-05-22 11:41:04
tags:
- meta
- what's new
---
After a long pause, MeasureThat.net got couple of new features and bug fixes. This post oulines some of them:

# Benchmark validation before saving
Writing correct code from the start is hard. We often make mistakes and need to make few iterations before finally getting it right. 
New feature `Validate benchmark` allows to run benchmark before saving it. 

Demo video: 
{% youtube T9dkykzScYk %}

# JS Memory measurements (Chrome only)
Chrome exposes `window.performance.memory` API which allows to track memory usage of the page. 
MeasureThat.net now can record memory consumption during benchmark execution and show results as chart.
Please note that for proper measurements Chrome must be launched with `--enable-precise-memory-info` flag.
More information about API: [trackjs.com: Monitoring JavaScript Memory](https://trackjs.com/blog/monitoring-javascript-memory/)

![Run tests and measure memory usage](https://user-images.githubusercontent.com/3173477/40384482-88d8fb66-5db8-11e8-8cbc-71b0fd94db8e.png)
![Results](https://user-images.githubusercontent.com/3173477/40384886-b885983c-5db9-11e8-8d38-80ed9c336737.png)


# Search
 New search widget allows to search for content using Google custom search.

![Google Custom Search](https://user-images.githubusercontent.com/3173477/40384552-b92d6fea-5db8-11e8-87d9-81a5c83db9c1.png)

# Blog
MeasureThat.net now features a blog (where this post is published).

# Do not allow duplicated titles
Benchmarks with duplicated titles are not prohibited. Please give descriptive and unique names to benchmarks.


# Bug fixs
There were multiple bug fixes and css optimizations.

As usual, please leave your feedback on the `Feedback & Suggestions` page or on project's GitHub page.