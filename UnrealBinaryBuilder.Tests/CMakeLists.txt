cmake_minimum_required(VERSION 3.10)

# Project name and version
project(UnrealBinaryBuilder.Tests LANGUAGES CSharp)

# Locate all source files
file(GLOB SOURCES "*.cs")

# Define the C# library for tests
add_library(UnrealBinaryBuilder.Tests SHARED
    ${SOURCES}
)

# Reference the main Avalonia project
add_dependencies(UnrealBinaryBuilder.Tests UnrealBinaryBuilder.Avalonia)

# Set common properties for C# library
set_property(TARGET UnrealBinaryBuilder.Tests PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v10.0")
set_property(TARGET UnrealBinaryBuilder.Tests PROPERTY VS_DOTNET_REFERENCES
    "System"
    "System.Core"
)

# Set output directory
set_target_properties(UnrealBinaryBuilder.Tests PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)

# Use the DOTNET_EXE approach for testing as well to handle NuGet dependencies
find_program(DOTNET_EXE NAMES dotnet)
if(DOTNET_EXE)
    add_custom_target(Test_UnrealBinaryBuilder
        COMMAND ${DOTNET_EXE} test ${CMAKE_CURRENT_SOURCE_DIR}/UnrealBinaryBuilder.Tests.csproj
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        SOURCES ${SOURCES}
    )
endif()
