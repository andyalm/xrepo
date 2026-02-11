_xrepo()
{
    local cur prev opts
    COMPREPLY=()
    cur="${COMP_WORDS[COMP_CWORD]}"
    prev="${COMP_WORDS[COMP_CWORD-1]}"

    opts="assemblies config link packages repo repos unlink which where"

    case "${prev}" in
      assemblies)
        return 0
        ;;
      config)
        COMPREPLY=()
        return 0
        ;;
      link)
        local repos=$(xrepo repos | grep ".* - .*" | awk -F " - " '{print $1}')
        COMPREPLY=($(compgen -W "${repos}" ${cur}))
        return 0
        ;;
      packages)
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
      unlink)
        local repos=$(xrepo repos | grep ".* - .*" | awk -F " - " '{print $1}')
        COMPREPLY=($(compgen -W "${repos}" ${cur}))
        return 0
        ;;
      which)
        local names=$(xrepo assemblies)
        COMPREPLY=($(compgen -W "${names}" ${cur}))
        return 0
        ;;
      where)
        local names=$(xrepo assemblies)
        COMPREPLY=($(compgen -W "${names}" ${cur}))
        return 0
        ;;
      *)
        COMPREPLY=($(compgen -W "${opts}" ${cur}))
        return 0
        ;;
    esac
}

complete -F _xrepo xrepo
