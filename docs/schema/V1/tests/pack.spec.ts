import { expect, test } from 'vitest';
import { promisify } from 'util';
import { exec } from 'child_process';
const execP = promisify(exec);

test('Snapshot test of `npm pack --dry-run --json`', async () => {
    const result = await execP('npm pack --dry-run --json');
    const json = JSON.parse(result.stdout);

    // Expect no errors while packing
    expect(result.stderr).toBe("");

    // Expect there to be one pack
    expect(json.length).toBe(1);

    // Expect the list of files to be the same as last time
    //  - Testing filename and file mode, while ignoring file size
    const files = json[0].files.map(({ path, mode }) => ({ path, mode }));
    expect(files).toMatchSnapshot();
});
