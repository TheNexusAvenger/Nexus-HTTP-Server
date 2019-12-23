# Nexus HTTP Server
Nexus HTTP Server is a light-weight .NET HTTP server implementation
loosely based on concepts of setting up a server in the
[Java Spark Microframework](http://sparkjava.com/). While not
fully featured like Java Spark or Spring Boot, the goal is to
provide a very simple class library for abstracting handling HTTP
requests from clients. This was originally implemneted in Nexus Git,
but was removed from the codebase for use in other applications.

## .NET Compatibility
The class library targets .NET Standard 2.0. For creating applications,
.NET Framework v4.6.1 or .NET Core 2.0 or newer is required. Check compatibility
for other versions of .NET if you are going to use it with something
else (like Mono or Xaramin).
