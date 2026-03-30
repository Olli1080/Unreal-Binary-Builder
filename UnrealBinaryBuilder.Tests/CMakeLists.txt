cmake_minimum_required(VERSION 3.10)

project(UnrealBinaryBuilder.Tests LANGUAGES NONE)

# Locate all source files
file(GLOB SOURCES "*.cs")

if(USE_NATIVE_CSHARP)
    # This generates a native .csproj inside the CMake solution (.sln) for tests
    enable_language(CSharp)
    add_library(UnrealBinaryBuilder.Tests SHARED ${SOURCES})
    
    set_property(TARGET UnrealBinaryBuilder.Tests PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v10.0")
    set_property(TARGET UnrealBinaryBuilder.Tests PROPERTY VS_DOTNET_REFERENCES "System" "System.Core")
    
    set_target_properties(UnrealBinaryBuilder.Tests PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    )
    
    add_dependencies(UnrealBinaryBuilder.Tests UnrealBinaryBuilder.Avalonia)
    
    # Custom target for running tests
    add_custom_target(Test_UnrealBinaryBuilder
        COMMAND ${DOTNET_EXE} test ${CMAKE_CURRENT_SOURCE_DIR}/UnrealBinaryBuilder.Tests.csproj
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        SOURCES ${SOURCES}
    )
else()
    # On Linux/macOS or non-VS generators, we use a custom target for tests
    add_custom_target(UnrealBinaryBuilder.Tests ALL
        COMMAND ${DOTNET_EXE} build ${CMAKE_CURRENT_SOURCE_DIR}/UnrealBinaryBuilder.Tests.csproj -c $<CONFIG> --no-restore
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        SOURCES ${SOURCES}
        COMMENT "Building UnrealBinaryBuilder.Tests using dotnet CLI..."
    )
    
    add_custom_target(Test_UnrealBinaryBuilder
        COMMAND ${DOTNET_EXE} test ${CMAKE_CURRENT_SOURCE_DIR}/UnrealBinaryBuilder.Tests.csproj
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        SOURCES ${SOURCES}
    )
    
    add_dependencies(UnrealBinaryBuilder.Tests UnrealBinaryBuilder.Avalonia)
endif()
