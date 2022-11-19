# Pull Request Guidelines

This section outlines the guidelines that should be imposed upon pull requests at Team Fortress Source 2.
From this point forward the abbreviation PR will be used in place of “pull request.”

Each PR will:
  1. Include a reference to the open GitHub issue it is addressing.
  2. Include a title that provides a one sentence overview of the purpose of the
     PR. Abbreviations can be used when necessary.
  3. Follow the template regardless of the nature of the PR.
  4. Be made against the dev branch for first time contributors and breaking changes.
  5. Be reviewed and approved by someone other than the author.

PRs that contain "low effort" changes **ONLY** such as spelling fixes, code comment additions/edits and formatting will not be accepted.  
Finally, PRs should be atomic. That is, they should address only one item (task, feature, bug).

## On Merging

Below are some general guidelines on how merging should be handled by the Amper Software development team.

First, a PR should not be merged to main/dev if any of the following apply:
  * A broken build which breaks the gamemode completely.
  * Comments asking for clarification that have not been addressed.
  * An explicit request to not merge by one of the developers.
  * A PR that was explicitly identified as a “work in progress”. 

Next, a PR should not be merged unless it has been reviewed by at least one
other person on the team.

## See Also

  * [Github - How to write perfect PR](https://github.com/blog/1943-how-to-write-the-perfect-pull-request)