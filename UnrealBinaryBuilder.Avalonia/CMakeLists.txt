cmake_minimum_required(VERSION 3.10)

project(UnrealBinaryBuilder.Avalonia LANGUAGES NONE)

# Sources for tracking
file(GLOB_RECURSE SOURCES
    "*.cs"
    "*.axaml"
    "*.axaml.cs"
)
list(APPEND SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/app.manifest")

# Icon generation
find_package(Python3 COMPONENTS Interpreter)
if(Python3_Interpreter_FOUND)
    # Ensure the Assets directory exists for the output
    file(MAKE_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/Assets")

    add_custom_command(
        OUTPUT "${CMAKE_CURRENT_SOURCE_DIR}/Assets/app_icon.ico"
        COMMAND "${Python3_EXECUTABLE}" "${CMAKE_SOURCE_DIR}/Scripts/generate_ico.py"
        DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/Assets/app_icon.svg" "${CMAKE_SOURCE_DIR}/Scripts/generate_ico.py"
        WORKING_DIRECTORY "${CMAKE_SOURCE_DIR}"
        COMMENT "Generating application icon from SVG..."
        VERBATIM
    )
    list(APPEND SOURCES "${CMAKE_CURRENT_SOURCE_DIR}/Assets/app_icon.ico")
endif()

if(USE_NATIVE_CSHARP)
    # This generates a native .csproj inside the CMake solution (.sln)
    enable_language(CSharp)
    add_executable(UnrealBinaryBuilder.Avalonia ${SOURCES})
    
    set_property(TARGET UnrealBinaryBuilder.Avalonia PROPERTY VS_DOTNET_TARGET_FRAMEWORK_VERSION "v10.0")
    set_property(TARGET UnrealBinaryBuilder.Avalonia PROPERTY VS_DOTNET_REFERENCES
        "System" "System.Core" "System.Data" "System.Runtime" "System.Xml" "System.Xml.Linq"
    )
    
    set_target_properties(UnrealBinaryBuilder.Avalonia PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    )
    
    add_dependencies(UnrealBinaryBuilder.Avalonia Restore)
else()
    # On Linux/macOS or non-VS generators, we use a custom target to build via dotnet
    set(OUTPUT_DIR "${CMAKE_BINARY_DIR}/bin")
    add_custom_target(UnrealBinaryBuilder.Avalonia ALL
        COMMAND ${DOTNET_EXE} build ${CMAKE_CURRENT_SOURCE_DIR}/UnrealBinaryBuilder.Avalonia.csproj -c $<CONFIG> -o ${OUTPUT_DIR} --no-restore
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        SOURCES ${SOURCES}
        COMMENT "Building UnrealBinaryBuilder.Avalonia using dotnet CLI..."
    )
    add_dependencies(UnrealBinaryBuilder.Avalonia Restore)
endif()

# Install targets
install(DIRECTORY "${CMAKE_BINARY_DIR}/bin/" DESTINATION bin)
