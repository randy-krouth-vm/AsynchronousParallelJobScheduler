# Asynchronous Parallel Job Scheduler

## Overview

**Asynchronous Parallel Job Scheduler** is a lightweight, priority-aware, parallel job scheduler designed for asynchronous workloads.

It supports:
- **Bounded parallel execution**
- **Hot / Cold job prioritization**
- **Cooperative cancellation**
- **Cold job re-queuing on preemption**

When a **hot job** arrives, currently running **cold jobs** are cooperatively cancelled and re-queued, allowing hot jobs to execute immediately.

This scheduler is designed for systems where **latency-sensitive operations must run immediately**, while background tasks can pause and resume safely.

---

## Key Concepts

### Cold Jobs
Cold jobs represent long-running or bulk operations.

- Can be cancelled safely
- Automatically re-queued on cancellation
- Resume execution later

Examples:
- Historical data downloads
- Bulk synchronization
- Background processing

---

### Hot Jobs
Hot jobs represent latency-sensitive operations.

- Always take priority over cold jobs
- Never re-queued
- Execute as soon as resources are available

Examples:
- Trade execution
- Real-time updates
- User-triggered actions

---

### Cooperative Preemption

Cold jobs are **not forcefully terminated**.

Instead:
1. A cancellation signal is issued
2. The job exits cooperatively
3. The job is placed back at the **front of the cold queue**
4. Execution resumes when resources are free

This avoids thread aborts and unsafe interruption.

---

## Architecture Summary

Each scheduler instance maintains:

- A **Cold Queue**
- A **Hot Queue**
- A list of **Active Cold Jobs**
- A `SemaphoreSlim` enforcing **bounded concurrency**

Multiple worker loops poll the queues and execute jobs.  
Hot jobs preempt cold jobs via cooperative cancellation and re-queueing.

---

## Features

- ✅ Async/await based execution
- ✅ Deterministic concurrency limits
- ✅ Priority scheduling (Hot > Cold)
- ✅ Cooperative cancellation
- ✅ Cold job re-queueing
- ✅ No thread blocking
- ✅ No task explosion
- ✅ Suitable for Web APIs and background services

<br>

<img width="1406" height="639" alt="image" src="https://github.com/user-attachments/assets/cfdb1413-3028-43b5-882c-498af170f1ee" />

