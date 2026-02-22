_dotnet_zsh_complete()
{
    # Get full path to script because dotnet-suggest needs it
    # NOTE: this requires a command registered with dotnet-suggest be
    # on the PATH
    full_path=`which ${words[1]}` # zsh arrays are 1-indexed

    # Get the completion results, will be newline-delimited
    # Use $BUFFER (raw edit buffer) and $CURSOR (cursor position) so that
    # dotnet-suggest can determine which token is being completed.
    completions=$(dotnet-suggest get --executable "$full_path" --position $CURSOR -- "$BUFFER")

    # explode the completions by linefeed instead of by spaces
    exploded=(${(f)completions})

    # Use compadd instead of _values because _values cannot handle
    # path-like completions containing '/' and errors on empty input.
    # Split into directory completions (ending with /) and other completions
    # so that directories don't append a trailing space after completion.
    local -a dirs nondirs
    for item in "${exploded[@]}"; do
        if [[ "$item" == */ ]]; then
            dirs+=("$item")
        else
            nondirs+=("$item")
        fi
    done

    local IFS=$'\n'
    (( ${#dirs} )) && compadd -Q -U -S '' -- "${dirs[@]}"
    (( ${#nondirs} )) && compadd -Q -- "${nondirs[@]}"
}

# apply this function to each command the dotnet-suggest knows about
compdef _dotnet_zsh_complete $(dotnet-suggest list)

export DOTNET_SUGGEST_SCRIPT_VERSION="1.0.0"
