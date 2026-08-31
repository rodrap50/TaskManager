interface Props {
    status: string;
}

const DOT: Record<string, string> = {
    Backlog:    'bg-gray-400',
    InProgress: 'bg-blue-500',
    Blocked:    'bg-red-500',
    Done:       'bg-green-500',
    Cancelled:  'bg-gray-300',
};

export function StatusIndicator({ status }: Props) {
    const dot = DOT[status] ?? DOT.Backlog;
    return <span className={`inline-block h-2.5 w-2.5 rounded-full ${dot}`} aria-label={status} />;
}
