# Optimal Route Calculation System (TSP Solver)

A desktop application designed to solve complex logistics and routing problems by integrating real-world map data with advanced graph algorithms. This system provides a visual and mathematical solution to the Traveling Salesperson Problem (TSP), ensuring global optimization for multi-stop routes.

## 🚀 Key Features

- **Dijkstra’s Algorithm:** Calculates the shortest path between any two points on the map.
- **Held-Karp Algorithm:** A dynamic programming approach to solve the TSP, guaranteeing the shortest possible route that visits all required waypoints.
- **Interactive Map UI:** Built with WPF, allowing users to visualize nodes, edges, and calculated routes in real-time.
- **Secure Authentication:** Implements SHA256 password hashing and secure login/registration flows.
- **Data Persistence:** Utilizes SQL Server and Entity Framework Core with optimized Eager Loading (.Include) for handling large graph structures.

## 🛠 Tech Stack

- **Language:** C#
- **Framework:** .NET 8 / WPF (Windows Presentation Foundation)
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Architecture:** MVVM (Model-View-ViewModel)

## 🧠 Algorithms Explained

### 1. Dijkstra’s Algorithm
Used for point-to-point navigation. It finds the shortest path from a source node to all other nodes in the graph. 
- **Complexity:** $O(E \log V)$

### 2. Held-Karp (Dynamic Programming)
Used for the Traveling Salesperson Problem. Unlike brute-force methods ($O(n!)$), Held-Karp uses bitmasking and memoization to find the optimal cycle.
- **Complexity:** $O(n^2 \cdot 2^n)$
- **Constraint:** Due to its exponential nature, the system limits the number of "mandatory stops" to ensure high performance.

## 📁 Project Structure

- `UI/`: WPF Windows, Views, and ViewModels.
- `Logic/`: Implementation of Graph algorithms and business logic.
- `Data/`: DB Context, Migrations, and Repository patterns.
- `Services/`: Authentication and background processing services.
