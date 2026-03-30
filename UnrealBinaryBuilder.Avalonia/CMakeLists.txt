cmake_minimum_required(VERSION 3.10)

# Project name and version
project(UnrealBinaryBuilder.Avalonia LANGUAGES CSharp)

# Locate all source files
file(GLOB_RECURSE SOURCES
    "*.cs"
    "*.axaml"
    "*.axaml.cs"
)

# Application manifest
list(APPEND SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/app.manifest")

# Icon generation
find_package(PythonInterp)
if(PYTHONINTERP_FOUND)
    set(VENV_DIR "${CMAKE_SOURCE_DIR}/.venv")
    if(WIN32)
        set(PYTHON_EXE "${VENV_DIR}/Scripts/python.exe")
    else()
        set(PYTHON_EXE "${VENV_DIR}/bin/python")
    endif()

    add_custom_command(
        OUTPUT "${CMAKE_CURRENT_SOURCE_DIR}/Assets/app_icon.ico"
        COMMAND ${PYTHON_EXECUTABLE} -m venv ${VENV_DIR}
        COMMAND ${PYTHON_EXE} ${CMAKE_SOURCE_DIR}/Scripts/generate_ico.py
        DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/Assets/app_icon.svg" "${CMAKE_SOURCE_DIR}/Scripts/generate_ico.py"
        COMMENT "Generating application icon from SVG..."
    )
    
    # Add the icon to the SOURCES to ensure it's generated
    list(APPEND SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/Assets/app_icon.ico")
endif()

# Define the C# executable
add_executable(UnrealBinaryBuilder.Avalonia
    ${SOURCES}
)

# Set common properties for C# executable
set_property(TARGET UnrealBinaryBuilder.Avalonia PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v10.0")
set_property(TARGET UnrealBinaryBuilder.Avalonia PROPERTY VS_DOTNET_REFERENCES
    "System"
    "System.Core"
    "System.Data"
    "System.Runtime"
    "System.Xml"
    "System.Xml.Linq"
)

# Set output directory
set_target_properties(UnrealBinaryBuilder.Avalonia PROPERTIES
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)

# Install targets
install(TARGETS UnrealBinaryBuilder.Avalonia
    DESTINATION bin
)

# Install assets and resources
install(DIRECTORY Assets/ DESTINATION bin/Assets)
install(DIRECTORY Resources/ DESTINATION bin/Resources)

# Since this is an Avalonia project, it's highly recommended to keep the .csproj 
# for Avalonia's specialized build tasks. We can configure CMake to wrap the 
# existing .csproj to ensure proper resource compilation and NuGet support.

# Add a custom command to build the existing project using dotnet
find_program(DOTNET_EXE NAMES dotnet)
if(DOTNET_EXE)
    add_custom_target(Build_UnrealBinaryBuilder ALL
        COMMAND ${DOTNET_EXE} build ${CMAKE_CURRENT_SOURCE_DIR}/UnrealBinaryBuilder.Avalonia.csproj
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        SOURCES ${SOURCES}
    )
endif()
