# MAPF
Multi-Agent Path Finding with CBS

## Building the project
The webpage for visualizing results is built using TypeScript which is a language that extends and compiles to JavaScript. For now, the compiled JavaScript is not tracked and therefore you must compile it locally to run the webpage. To do this, first [install typescript](https://www.typescriptlang.org/download) and then run from the root of the project
`tsc.cmd --build html/scripts/tsconfig.json`
(note that the `.cmd` should not be necessary but is required on Windows because of an [issue](https://github.com/npm/cli/issues/470) related to pipes and PowerShell)
