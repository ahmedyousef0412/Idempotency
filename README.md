# Idempotency
This project demonstrates how to implement **Idempotency** in a Web API to prevent duplicate processing of requests, specifically focusing on the **Idempotency Key** pattern.


## 🚀 The Problem
In distributed systems, retries are inevitable. Without idempotency, a retried "Create Order" request could result in multiple charges or duplicate database entries. This demo solves the "Double-Spend" or "Double-Post" problem.
