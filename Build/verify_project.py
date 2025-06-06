import os
import sys


def main():
    if len(sys.argv) < 2:
        print(
            "Needs one argument: csproj name minus .csproj. Previous GitHub Action step shouldn't have allowed us to get to this point.",
            file=sys.stderr)
        sys.exit(1)

    project_name = sys.argv[1]
    found_project, project_path = get_project_path(project_name)

    if not found_project:
        print(f"Couldn't find the specified project {sys.argv[1]}")
        sys.exit(1)

    print(f"Found csproj: {project_path}")
    print_to_github_output(project_path)


def get_project_path(project_name: str) -> tuple[bool, str]:
    """
    Get the absolute path to the project's csproj file.
    :param project_name: The name of the csproj file for the project (minus the .csproj).
    :return: A tuple of True/False for success, and a string of the absolute path to the project file (with .csproj).
    """

    current_dir = os.getcwd()
    print(f"Searching in {current_dir}")

    try:
        for root, dir, files in os.walk(current_dir):
            for file in files:
                if file.endswith("csproj"):
                    if file == f"{project_name}.csproj":
                        file_path = os.path.join(root, file)
                        return (True, file_path)
    except IOError:
        print("Caught IOError.")
        sys.exit(1)

    return (False, "")

def print_to_github_output(project_path: str):
    """
    Output required information to GitHub Actions output.
    :param project_path: Absolute path to the csproj project file
    :return: None
    """
    step_output_file = os.environ.get("GITHUB_OUTPUT")

    if step_output_file:
        with open(step_output_file, "w") as output_file:
            print(f"Setting step output absolute_csproj_path={project_path}")
            print(f"absolute_csproj_path={project_path}", file=output_file)
            print(f"Setting step output csproj_dir={os.path.dirname(project_path)}")
            print(f"csproj_dir={os.path.dirname(project_path)}", file=output_file)
    else:
        print(f"GitHub Actions output file not found: {step_output_file}")
        sys.exit(1)


if __name__ == "__main__":
    main()
