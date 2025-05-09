# 🔐 Secure File Sharing with SAS Tokens in an Event-Driven Architecture

This project demonstrates how to securely share files using **Azure Blob Storage SAS (Shared Access Signature) tokens** within an **event-driven microservices architecture**. The system is built with two key microservices—one for document generation and another for file management—coordinated through **Azure Service Bus**.

![Architecture Overview]
<img width="530" alt="image" src="https://github.com/user-attachments/assets/808e3dd0-6ee8-4e73-8541-84f5bf648ede" />
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
