# regexDirScan
Recursive scaning for regex pattern in directory names, extracting submatches and sorting paths only to matched directory level lexicagraphicaly by submatches.
If submatch is number, then converting it to integer before comparison.
First argument should be directory for scanning.
Second argument should be regexp with submatches for comparison and by default is (\d+)\.(\d+)\.(\d+)\.(\d+)
