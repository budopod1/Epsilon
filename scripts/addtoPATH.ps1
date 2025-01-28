$context = $args[1]
$old_path = [Environment]::GetEnvironmentVariable('PATH', $context)
$dir_to_add = Resolve-Path $args[0]
$new_path = $old_path + [IO.Path]::PathSeparator + $dir_to_add.ToString()
[Environment]::SetEnvironmentVariable('PATH', $new_path, $context)
