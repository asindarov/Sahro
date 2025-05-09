# 🔐 Secure File Sharing with SAS Tokens in an Event-Driven Architecture

This project demonstrates how to securely share files using **Azure Blob Storage SAS (Shared Access Signature) tokens** within an **event-driven microservices architecture**. The system is built with two key microservices—one for document generation and another for file management—coordinated through **Azure Service Bus**.

## Event Driven Architecture overview 
<img width="613" alt="image" src="https://github.com/user-attachments/assets/6e621707-4955-45ea-9d97-4a1d5a06b781" />

<!-- Replace with your image path -->

## 📘 Project Summary

**Goal:**  
Protect uploaded documents and manage access securely using expiring SAS tokens, while enabling scalable asynchronous communication between services through Azure messaging.

**Highlights:**
- Fine-grained file access using Azure SAS tokens
- Integration of microservices in an event-driven setup
- Document generation from user input
- Secure file upload and download
- SAS token lifecycle and access control

## 🧱 System Architecture

```mermaid
graph TD
    A[User Input] --> B[Document Generation Service]
    B --> C[DocumentCreated Event]
    C --> D[Document Worker Service]
    D --> E[File Management Service]
    E --> F[Azure Blob Storage]
    F --> G[Download with SAS Token]
