interface Props {
    displayName: string;
    avatarUrl?: string | null;
    size?: 'sm' | 'md';
}

const COLORS = [
    'bg-purple-500', 'bg-blue-500', 'bg-green-500',
    'bg-yellow-500', 'bg-red-500', 'bg-pink-500',
];

function colorFor(name: string) {
    let hash = 0;
    for (const ch of name) hash = (hash * 31 + ch.charCodeAt(0)) & 0xffff;
    return COLORS[hash % COLORS.length];
}

export function AssigneeAvatar({ displayName, avatarUrl, size = 'sm' }: Props) {
    const dim = size === 'sm' ? 'h-6 w-6 text-xs' : 'h-9 w-9 text-sm';
    const initials = displayName
        .split(' ')
        .slice(0, 2)
        .map(w => w[0])
        .join('')
        .toUpperCase();

    if (avatarUrl) {
        return (
            <img
                src={avatarUrl}
                alt={displayName}
                className={`${dim} rounded-full object-cover`}
                title={displayName}
            />
        );
    }

    return (
        <span
            className={`${dim} ${colorFor(displayName)} inline-flex items-center justify-center rounded-full font-medium text-white`}
            title={displayName}
        >
            {initials}
        </span>
    );
}
