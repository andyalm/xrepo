_xrepo()
{
    local cur prev opts
    COMPREPLY=()
    cur="${COMP_WORDS[COMP_CWORD]}"
    prev="${COMP_WORDS[COMP_CWORD-1]}"

    opts="assemblies config pin pins repo repos unpin"

    case "${prev}" in
      assemblies)
        return 0
        ;;
      config)
        local config_options="copy_pins pin_warnings auto_build_pins"
        COMPREPLY=($(compgen -W "${config_options}" ${cur}))
        return 0
        ;;
      pin)
        local repos=$(xrepo repos | grep ".* - .*" | awk -F " - " '{print $1}')
        COMPREPLY=($(compgen -W "${repos}" ${cur}))
        return 0
        ;;
      pins)
        return 0
        ;;
      repo)
        local repo_options="register unregister -p --path"
        COMPREPLY=($(compgen -W "${repo_options}" -- ${cur}))
        return 0
        ;;
      repos)
        return 0
        ;;
      unpin)
        local pinned_repos=$(xrepo pins | grep -v "pinned *" | grep -v "\-" | grep -v -e '^$')
        COMPREPLY=($(compgen -W "${pinned_repos}" ${cur}))
        return 0
        ;;
      *)
        COMPREPLY=($(compgen -W "${opts}" ${cur}))  
        return 0
        ;;
    esac
}

complete -F _xrepo xrepo
